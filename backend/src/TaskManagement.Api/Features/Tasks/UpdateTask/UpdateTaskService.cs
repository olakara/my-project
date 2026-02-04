using TaskManagement.Api.Data;
using TaskManagement.Api.Data.Repositories;
using TaskManagement.Api.Domain.Projects;
using TaskManagement.Api.Domain.Tasks;
using TaskManagement.Api.Domain.Users;
using TaskManagement.Api.Features.Tasks.GetTask;
using DomainTask = TaskManagement.Api.Domain.Tasks.Task;

namespace TaskManagement.Api.Features.Tasks.UpdateTask;

public interface IUpdateTaskService
{
    System.Threading.Tasks.Task<GetTaskResponse> UpdateTaskAsync(int taskId, string userId, UpdateTaskRequest request, CancellationToken ct = default);
}

public class UpdateTaskService : IUpdateTaskService
{
    private const int HistoryPreviewLimit = 5;
    private readonly TaskManagementDbContext _context;
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<UpdateTaskService> _logger;

    public UpdateTaskService(
        TaskManagementDbContext context,
        ITaskRepository taskRepository,
        IProjectRepository projectRepository,
        ILogger<UpdateTaskService> logger)
    {
        _context = context;
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<GetTaskResponse> UpdateTaskAsync(int taskId, string userId, UpdateTaskRequest request, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetByIdAsync(taskId, ct);
        if (task == null)
        {
            throw new KeyNotFoundException("Task not found");
        }

        var project = await _projectRepository.GetByIdAsync(task.ProjectId, ct);
        if (project == null)
        {
            throw new KeyNotFoundException("Project not found");
        }

        if (!CanEditTask(task, project, userId))
        {
            throw new UnauthorizedAccessException("User is not authorized to update this task");
        }

        var now = DateTime.UtcNow;
        var historyEntries = BuildHistoryEntries(task, request, userId, ResolveUser(project, userId), now);

        if (historyEntries.Count == 0)
        {
            return BuildResponse(task, project, historyEntries);
        }

        ApplyUpdates(task, request, now);

        _context.TaskHistory.AddRange(historyEntries);
        await _taskRepository.UpdateAsync(task, ct);

        _logger.LogInformation("Task {TaskId} updated by user {UserId}", task.Id, userId);

        return BuildResponse(task, project, historyEntries);
    }

    private static void ApplyUpdates(DomainTask task, UpdateTaskRequest request, DateTime timestamp)
    {
        if (request.Title != null)
        {
            task.Title = request.Title.Trim();
        }

        if (request.Description != null)
        {
            task.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        }

        if (request.Priority.HasValue)
        {
            task.Priority = request.Priority.Value;
        }

        if (request.DueDate != task.DueDate)
        {
            task.DueDate = request.DueDate;
        }

        task.UpdatedTimestamp = timestamp;
    }

    private static List<TaskHistory> BuildHistoryEntries(
        DomainTask task,
        UpdateTaskRequest request,
        string userId,
        ApplicationUser? user,
        DateTime timestamp)
    {
        var entries = new List<TaskHistory>();

        if (request.Title != null)
        {
            var newTitle = request.Title.Trim();
            if (!string.Equals(task.Title, newTitle, StringComparison.Ordinal))
            {
                entries.Add(CreateHistory(task.Id, userId, user, TaskHistoryChangeType.TitleChanged, task.Title, newTitle, timestamp));
            }
        }

        if (request.Description != null)
        {
            var newDescription = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            if (!string.Equals(task.Description, newDescription, StringComparison.Ordinal))
            {
                entries.Add(CreateHistory(task.Id, userId, user, TaskHistoryChangeType.DescriptionChanged, task.Description, newDescription, timestamp));
            }
        }

        if (request.Priority.HasValue && task.Priority != request.Priority.Value)
        {
            entries.Add(CreateHistory(
                task.Id,
                userId,
                user,
                TaskHistoryChangeType.PriorityChanged,
                task.Priority.ToString(),
                request.Priority.Value.ToString(),
                timestamp));
        }

        if (request.DueDate != task.DueDate)
        {
            entries.Add(CreateHistory(
                task.Id,
                userId,
                user,
                TaskHistoryChangeType.DueDateChanged,
                task.DueDate?.ToString("O"),
                request.DueDate?.ToString("O"),
                timestamp));
        }

        return entries;
    }

    private static TaskHistory CreateHistory(
        int taskId,
        string userId,
        ApplicationUser? user,
        TaskHistoryChangeType changeType,
        string? oldValue,
        string? newValue,
        DateTime timestamp)
    {
        return new TaskHistory
        {
            TaskId = taskId,
            ChangedBy = userId,
            ChangedByUser = user ?? new ApplicationUser { Id = userId },
            ChangeType = changeType,
            OldValue = oldValue,
            NewValue = newValue,
            ChangedTimestamp = timestamp
        };
    }

    private static bool CanEditTask(DomainTask task, Project project, string userId)
    {
        if (task.CreatedBy == userId)
        {
            return true;
        }

        if (project.OwnerId == userId)
        {
            return true;
        }

        var role = project.GetUserRole(userId);
        return role == ProjectRole.Manager;
    }

    private static ApplicationUser? ResolveUser(Project project, string userId)
    {
        if (project.OwnerId == userId)
        {
            return project.Owner;
        }

        return project.Members.FirstOrDefault(m => m.UserId == userId)?.User;
    }

    private static GetTaskResponse BuildResponse(DomainTask task, Project project, List<TaskHistory> newHistoryEntries)
    {
        var comments = task.Comments
            .OrderBy(c => c.CreatedTimestamp)
            .Select(comment => new CommentResponse
            {
                Id = comment.Id,
                Content = comment.Content,
                Author = BuildUserSummary(comment.Author),
                CreatedAt = comment.CreatedTimestamp,
                EditedAt = comment.EditedTimestamp
            })
            .ToList();

        var historyPreview = task.History
            .Concat(newHistoryEntries)
            .OrderByDescending(h => h.ChangedTimestamp)
            .Take(HistoryPreviewLimit)
            .Select(history => new TaskHistoryResponse
            {
                Id = history.Id,
                ChangeType = history.ChangeType,
                OldValue = history.OldValue,
                NewValue = history.NewValue,
                ChangedBy = BuildUserSummary(history.ChangedByUser, history.ChangedBy),
                ChangedAt = history.ChangedTimestamp
            })
            .ToList();

        return new GetTaskResponse
        {
            Id = task.Id,
            ProjectId = project.Id,
            ProjectName = project.Name,
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
            IsOverdue = task.IsOverdue,
            Comments = comments,
            HistoryPreview = historyPreview
        };
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
