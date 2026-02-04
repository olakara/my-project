using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.Data.Repositories;
using TaskManagement.Api.Domain.Projects;
using TaskManagement.Api.Domain.Tasks;
using DomainTask = TaskManagement.Api.Domain.Tasks.Task;
using DomainTaskStatus = TaskManagement.Api.Domain.Tasks.TaskStatus;

namespace TaskManagement.Api.Features.Tasks.UpdateTaskStatus;

public interface IUpdateTaskStatusService
{
    System.Threading.Tasks.Task<UpdateTaskStatusResponse> UpdateTaskStatusAsync(
        int taskId,
        string userId,
        UpdateTaskStatusRequest request,
        CancellationToken ct = default);
}

public class UpdateTaskStatusService : IUpdateTaskStatusService
{
    private readonly TaskManagementDbContext _context;
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<UpdateTaskStatusService> _logger;

    public UpdateTaskStatusService(
        TaskManagementDbContext context,
        ITaskRepository taskRepository,
        IProjectRepository projectRepository,
        ILogger<UpdateTaskStatusService> logger)
    {
        _context = context;
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<UpdateTaskStatusResponse> UpdateTaskStatusAsync(
        int taskId,
        string userId,
        UpdateTaskStatusRequest request,
        CancellationToken ct = default)
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

        // Check if user is a project member
        var isMember = await _context.ProjectMembers
            .AsNoTracking()
            .AnyAsync(pm => pm.ProjectId == project.Id && pm.UserId == userId, ct);

        if (!isMember)
        {
            throw new UnauthorizedAccessException("User is not a member of this project");
        }

        // Skip if status hasn't changed
        if (task.Status == request.NewStatus)
        {
            return BuildResponse(task);
        }

        // Record history entry for status change
        var now = DateTime.UtcNow;
        var oldStatus = task.Status;
        task.Status = request.NewStatus;
        task.UpdatedTimestamp = now;

        var historyEntry = new TaskHistory
        {
            TaskId = taskId,
            ChangeType = TaskHistoryChangeType.StatusChanged,
            OldValue = oldStatus.ToString(),
            NewValue = request.NewStatus.ToString(),
            ChangedBy = userId,
            ChangedTimestamp = now
        };

        _context.TaskHistory.Add(historyEntry);
        await _taskRepository.UpdateAsync(task, ct);

        _logger.LogInformation(
            "Task {TaskId} status changed from {OldStatus} to {NewStatus} by user {UserId}",
            taskId, oldStatus, request.NewStatus, userId);

        // TODO: Broadcast update via SignalR hub
        // await _signalRHub.BroadcastTaskStatusChanged(project.Id, task);

        return BuildResponse(task);
    }

    private UpdateTaskStatusResponse BuildResponse(DomainTask task)
    {
        return new UpdateTaskStatusResponse
        {
            Id = task.Id,
            Title = task.Title,
            Status = task.Status,
            AssigneeName = task.Assignee != null 
                ? $"{task.Assignee.FirstName} {task.Assignee.LastName}".Trim()
                : null,
            UpdatedTimestamp = task.UpdatedTimestamp
        };
    }
}
