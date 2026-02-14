using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.Data.Repositories;
using TaskManagement.Api.Domain.Projects;
using TaskManagement.Api.Domain.Tasks;
using TaskManagement.Api.Domain.Users;

namespace TaskManagement.Api.Features.Dashboard.GetTeamActivity;

public record TeamActivityMemberDto(
    string UserId,
    string FullName,
    string Email,
    string Role,
    int AssignedTasks,
    int CompletedTasks);

public record TeamActivityResponse(
    int ProjectId,
    string ProjectName,
    int TotalCompletedTasks,
    IReadOnlyList<TeamActivityMemberDto> Members);

public interface IGetTeamActivityService
{
    System.Threading.Tasks.Task<TeamActivityResponse> GetTeamActivityAsync(int projectId, string userId, CancellationToken ct = default);
}

public class GetTeamActivityService : IGetTeamActivityService
{
    private readonly TaskManagementDbContext _context;
    private readonly IProjectRepository _projectRepository;

    public GetTeamActivityService(TaskManagementDbContext context, IProjectRepository projectRepository)
    {
        _context = context;
        _projectRepository = projectRepository;
    }

    public async System.Threading.Tasks.Task<TeamActivityResponse> GetTeamActivityAsync(
        int projectId,
        string userId,
        CancellationToken ct = default)
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

        var assignedStats = await _context.Tasks
            .AsNoTracking()
            .Where(task => task.ProjectId == projectId && task.AssigneeId != null)
            .GroupBy(task => task.AssigneeId!)
            .Select(group => new { UserId = group.Key, Count = group.Count() })
            .ToListAsync(ct);

        var completedStats = await _context.TaskHistory
            .AsNoTracking()
            .Where(history => history.Task.ProjectId == projectId
                && history.ChangeType == TaskHistoryChangeType.StatusChanged
                && history.NewValue == TaskStatus.Done.ToString())
            .GroupBy(history => history.ChangedBy)
            .Select(group => new
            {
                UserId = group.Key,
                Count = group.Select(entry => entry.TaskId).Distinct().Count()
            })
            .ToListAsync(ct);

        var assignedLookup = assignedStats.ToDictionary(entry => entry.UserId, entry => entry.Count);
        var completedLookup = completedStats.ToDictionary(entry => entry.UserId, entry => entry.Count);

        var members = project.Members
            .Select(member =>
            {
                assignedLookup.TryGetValue(member.UserId, out var assigned);
                completedLookup.TryGetValue(member.UserId, out var completed);

                return new TeamActivityMemberDto(
                    UserId: member.UserId,
                    FullName: BuildFullName(member.User),
                    Email: member.User.Email ?? string.Empty,
                    Role: member.Role.ToString(),
                    AssignedTasks: assigned,
                    CompletedTasks: completed);
            })
            .OrderByDescending(member => member.CompletedTasks)
            .ThenByDescending(member => member.AssignedTasks)
            .ThenBy(member => member.FullName)
            .ToList();

        var totalCompleted = completedStats.Sum(entry => entry.Count);

        return new TeamActivityResponse(
            ProjectId: project.Id,
            ProjectName: project.Name,
            TotalCompletedTasks: totalCompleted,
            Members: members);
    }

    private static bool IsProjectMember(Project project, string userId)
    {
        if (project.OwnerId == userId)
        {
            return true;
        }

        return project.Members.Any(member => member.UserId == userId);
    }

    private static string BuildFullName(ApplicationUser user)
    {
        var parts = new[] { user.FirstName, user.LastName }
            .Where(part => !string.IsNullOrWhiteSpace(part));

        return string.Join(" ", parts);
    }
}
