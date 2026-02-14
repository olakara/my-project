using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.Domain.Tasks;
using TaskManagement.Api.Domain.Users;
using TaskManagement.Api.Features.Tasks.GetTask;
using DomainTaskStatus = TaskManagement.Api.Domain.Tasks.TaskStatus;

namespace TaskManagement.Api.Features.Tasks.GetMyTasks;

public interface IGetMyTasksService
{
    System.Threading.Tasks.Task<List<GetMyTasksResponse>> GetMyTasksAsync(
        string userId,
        int page = 1,
        int pageSize = 50,
        DomainTaskStatus? statusFilter = null,
        TaskPriority? priorityFilter = null,
        CancellationToken ct = default);
}

public class GetMyTasksService : IGetMyTasksService
{
    private readonly TaskManagementDbContext _context;
    private readonly ILogger<GetMyTasksService> _logger;

    public GetMyTasksService(TaskManagementDbContext context, ILogger<GetMyTasksService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<List<GetMyTasksResponse>> GetMyTasksAsync(
        string userId,
        int page = 1,
        int pageSize = 50,
        DomainTaskStatus? statusFilter = null,
        TaskPriority? priorityFilter = null,
        CancellationToken ct = default)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1 || pageSize > 100)
        {
            pageSize = 50;
        }

        var query = _context.Tasks
            .AsNoTracking()
            .Where(t => t.AssigneeId == userId);

        if (statusFilter.HasValue)
        {
            query = query.Where(t => t.Status == statusFilter.Value);
        }

        if (priorityFilter.HasValue)
        {
            query = query.Where(t => t.Priority == priorityFilter.Value);
        }

        var tasks = await query
            .Include(t => t.Project)
            .Include(t => t.Creator)
            .Include(t => t.Assignee)
            .Include(t => t.Comments)
            .OrderByDescending(t => t.UpdatedTimestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var results = tasks
            .Select(task => new GetMyTasksResponse
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                ProjectName = task.Project?.Name ?? string.Empty,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                Assignee = task.Assignee == null ? null : BuildUserSummary(task.Assignee),
                CreatedBy = BuildUserSummary(task.Creator),
                DueDate = task.DueDate,
                CreatedAt = task.CreatedTimestamp,
                UpdatedAt = task.UpdatedTimestamp,
                CommentCount = task.Comments.Count,
                IsOverdue = task.IsOverdue
            })
            .ToList();

        _logger.LogInformation("Retrieved {TaskCount} tasks for user {UserId}", results.Count, userId);

        return results;
    }

    private static UserSummaryResponse BuildUserSummary(ApplicationUser user)
    {
        return BuildUserSummary(user, user.Id);
    }

    private static UserSummaryResponse BuildUserSummary(ApplicationUser? user, string userId)
    {
        var fullName = user == null
            ? string.Empty
            : string.Join(" ", new[] { user.FirstName, user.LastName }.Where(n => !string.IsNullOrWhiteSpace(n)));

        return new UserSummaryResponse
        {
            UserId = userId,
            FullName = fullName,
            Email = user?.Email ?? string.Empty,
            ProfilePictureUrl = user?.ProfilePictureUrl
        };
    }
}
