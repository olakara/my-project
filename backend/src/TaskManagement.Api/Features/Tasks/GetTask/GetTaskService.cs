using TaskManagement.Api.Data.Repositories;
using TaskManagement.Api.Domain.Projects;
using TaskManagement.Api.Domain.Users;

namespace TaskManagement.Api.Features.Tasks.GetTask;

public interface IGetTaskService
{
    System.Threading.Tasks.Task<GetTaskResponse> GetTaskAsync(int taskId, string userId, CancellationToken ct = default);
}

public class GetTaskService : IGetTaskService
{
    private const int HistoryPreviewLimit = 5;
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;

    public GetTaskService(ITaskRepository taskRepository, IProjectRepository projectRepository)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
    }

    public async System.Threading.Tasks.Task<GetTaskResponse> GetTaskAsync(int taskId, string userId, CancellationToken ct = default)
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

        if (!IsProjectMember(project, userId))
        {
            throw new UnauthorizedAccessException("User is not authorized to view this task");
        }

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

    private static bool IsProjectMember(Project project, string userId)
    {
        if (project.OwnerId == userId)
        {
            return true;
        }

        return project.Members.Any(m => m.UserId == userId);
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
