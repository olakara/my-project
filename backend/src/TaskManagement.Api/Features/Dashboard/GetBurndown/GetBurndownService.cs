using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.Data.Repositories;
using TaskManagement.Api.Domain.Projects;
using TaskManagement.Api.Domain.Tasks;
using TaskManagement.Api.Domain.Users;

namespace TaskManagement.Api.Features.Dashboard.GetBurndown;

public record BurndownDayDto(DateTime Date, int CompletedTasks);

public record BurndownResponse(
    int ProjectId,
    string ProjectName,
    DateTime StartDate,
    DateTime EndDate,
    int TotalCompleted,
    IReadOnlyList<BurndownDayDto> Days);

public interface IGetBurndownService
{
    System.Threading.Tasks.Task<BurndownResponse> GetBurndownAsync(
        int projectId,
        string userId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct = default);
}

public class GetBurndownService : IGetBurndownService
{
    private readonly TaskManagementDbContext _context;
    private readonly IProjectRepository _projectRepository;

    public GetBurndownService(TaskManagementDbContext context, IProjectRepository projectRepository)
    {
        _context = context;
        _projectRepository = projectRepository;
    }

    public async System.Threading.Tasks.Task<BurndownResponse> GetBurndownAsync(
        int projectId,
        string userId,
        DateTime startDate,
        DateTime endDate,
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

        var start = startDate.Date;
        var end = endDate.Date;
        var endExclusive = end.AddDays(1);

        var completionEvents = await _context.TaskHistory
            .AsNoTracking()
            .Where(history => history.Task.ProjectId == projectId
                && history.ChangeType == TaskHistoryChangeType.StatusChanged
                && history.NewValue == TaskStatus.Done.ToString()
                && history.ChangedTimestamp >= start
                && history.ChangedTimestamp < endExclusive)
            .GroupBy(history => history.ChangedTimestamp.Date)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToListAsync(ct);

        var completionLookup = completionEvents.ToDictionary(entry => entry.Key, entry => entry.Count);
        var days = new List<BurndownDayDto>();

        for (var date = start; date <= end; date = date.AddDays(1))
        {
            completionLookup.TryGetValue(date, out var count);
            days.Add(new BurndownDayDto(date, count));
        }

        var totalCompleted = days.Sum(day => day.CompletedTasks);

        return new BurndownResponse(
            ProjectId: project.Id,
            ProjectName: project.Name,
            StartDate: start,
            EndDate: end,
            TotalCompleted: totalCompleted,
            Days: days);
    }

    private static bool IsProjectMember(Project project, string userId)
    {
        if (project.OwnerId == userId)
        {
            return true;
        }

        return project.Members.Any(member => member.UserId == userId);
    }
}
