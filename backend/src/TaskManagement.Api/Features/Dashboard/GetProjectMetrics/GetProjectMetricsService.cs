using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.Data.Repositories;
using TaskManagement.Api.Domain.Projects;
using DomainTaskStatus = TaskManagement.Api.Domain.Tasks.TaskStatus;
using TaskManagement.Api.Domain.Users;

namespace TaskManagement.Api.Features.Dashboard.GetProjectMetrics;

public record StatusCountDto(DomainTaskStatus Status, int Count);

public record TeamMemberMetricsDto(
    string UserId,
    string FullName,
    string Email,
    string Role,
    int AssignedTasks,
    int CompletedTasks);

public record ProjectMetricsResponse(
    int ProjectId,
    string ProjectName,
    int TotalTasks,
    int CompletedTasks,
    decimal CompletionPercentage,
    IReadOnlyList<StatusCountDto> StatusCounts,
    IReadOnlyList<TeamMemberMetricsDto> TeamMembers);

public interface IGetProjectMetricsService
{
    System.Threading.Tasks.Task<ProjectMetricsResponse> GetProjectMetricsAsync(int projectId, string userId, CancellationToken ct = default);
}

public class GetProjectMetricsService : IGetProjectMetricsService
{
    private readonly TaskManagementDbContext _context;
    private readonly IProjectRepository _projectRepository;

    public GetProjectMetricsService(TaskManagementDbContext context, IProjectRepository projectRepository)
    {
        _context = context;
        _projectRepository = projectRepository;
    }

    public async System.Threading.Tasks.Task<ProjectMetricsResponse> GetProjectMetricsAsync(int projectId, string userId, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, ct);
        if (project == null)
        {
            throw new KeyNotFoundException("Project not found");
        }

        if (!IsProjectMember(project, userId))
        {
            throw new UnauthorizedAccessException("User is not a member of this project");
        }

        var statusCounts = await _context.Tasks
            .AsNoTracking()
            .Where(task => task.ProjectId == projectId)
            .GroupBy(task => task.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToListAsync(ct);

        var totalTasks = statusCounts.Sum(entry => entry.Count);
        var completedTasks = statusCounts.FirstOrDefault(entry => entry.Status == DomainTaskStatus.Done)?.Count ?? 0;
        var completionPercentage = totalTasks == 0
            ? 0m
            : Math.Round(completedTasks * 100m / totalTasks, 2);

        var statusCountDtos = Enum.GetValues(typeof(DomainTaskStatus))
            .Cast<DomainTaskStatus>()
            .Select(status => new StatusCountDto(
                status,
                statusCounts.FirstOrDefault(entry => entry.Status == status)?.Count ?? 0))
            .ToList();

        var assigneeStats = await _context.Tasks
            .AsNoTracking()
            .Where(task => task.ProjectId == projectId && task.AssigneeId != null)
            .GroupBy(task => task.AssigneeId!)
            .Select(group => new
            {
                AssigneeId = group.Key,
                AssignedCount = group.Count(),
                CompletedCount = group.Count(task => task.Status == DomainTaskStatus.Done)
            })
            .ToListAsync(ct);

        var assigneeLookup = assigneeStats.ToDictionary(entry => entry.AssigneeId, entry => entry);

        var teamMembers = project.Members
            .OrderBy(member => RoleOrder(member.Role))
            .ThenBy(member => member.User.LastName ?? string.Empty)
            .ThenBy(member => member.User.FirstName ?? string.Empty)
            .Select(member =>
            {
                var stats = assigneeLookup.TryGetValue(member.UserId, out var entry) ? entry : null;

                return new TeamMemberMetricsDto(
                    UserId: member.UserId,
                    FullName: BuildFullName(member.User),
                    Email: member.User.Email ?? string.Empty,
                    Role: member.Role.ToString(),
                    AssignedTasks: stats?.AssignedCount ?? 0,
                    CompletedTasks: stats?.CompletedCount ?? 0);
            })
            .ToList();

        return new ProjectMetricsResponse(
            ProjectId: project.Id,
            ProjectName: project.Name,
            TotalTasks: totalTasks,
            CompletedTasks: completedTasks,
            CompletionPercentage: completionPercentage,
            StatusCounts: statusCountDtos,
            TeamMembers: teamMembers);
    }

    private static bool IsProjectMember(Project project, string userId)
    {
        if (project.OwnerId == userId)
        {
            return true;
        }

        return project.Members.Any(member => member.UserId == userId);
    }

    private static int RoleOrder(ProjectRole role)
    {
        return role switch
        {
            ProjectRole.Owner => 0,
            ProjectRole.Manager => 1,
            _ => 2
        };
    }

    private static string BuildFullName(ApplicationUser user)
    {
        var parts = new[] { user.FirstName, user.LastName }
            .Where(part => !string.IsNullOrWhiteSpace(part));

        return string.Join(" ", parts);
    }
}
