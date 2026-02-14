using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.Data.Repositories;
using TaskManagement.Api.Domain.Notifications;
using TaskManagement.Api.Domain.Projects;
using TaskManagement.Api.Domain.Tasks;
using TaskManagement.Api.Domain.Users;
using TaskManagement.Api.Hubs;
using DomainTask = TaskManagement.Api.Domain.Tasks.Task;

namespace TaskManagement.Api.Features.Tasks.AssignTask;

public interface IAssignTaskService
{
    System.Threading.Tasks.Task<AssignTaskResponse> AssignTaskAsync(
        int taskId,
        string userId,
        AssignTaskRequest request,
        CancellationToken ct = default);
}

public class AssignTaskService : IAssignTaskService
{
    private readonly TaskManagementDbContext _context;
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IHubContext<TaskManagementHub> _hubContext;
    private readonly ILogger<AssignTaskService> _logger;

    public AssignTaskService(
        TaskManagementDbContext context,
        ITaskRepository taskRepository,
        IProjectRepository projectRepository,
        IHubContext<TaskManagementHub> hubContext,
        ILogger<AssignTaskService> logger)
    {
        _context = context;
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<AssignTaskResponse> AssignTaskAsync(
        int taskId,
        string userId,
        AssignTaskRequest request,
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
        var userMember = await _context.ProjectMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(pm => pm.ProjectId == project.Id && pm.UserId == userId, ct);

        if (userMember == null)
        {
            throw new UnauthorizedAccessException("User is not a member of this project");
        }

        // Check authorization: only Owner/Manager or task creator can assign
        var isOwnerOrManager = userMember.Role == ProjectRole.Owner || userMember.Role == ProjectRole.Manager;
        var isTaskCreator = task.CreatedBy == userId;

        if (!isOwnerOrManager && !isTaskCreator)
        {
            throw new UnauthorizedAccessException("Only project managers, owners, or task creator can assign tasks");
        }

        // Validate assignee if provided
        var newAssigneeId = string.IsNullOrWhiteSpace(request.AssigneeId) ? null : request.AssigneeId.Trim();

        if (!string.IsNullOrEmpty(newAssigneeId))
        {
            var assigneeMember = await _context.ProjectMembers
                .AsNoTracking()
                .FirstOrDefaultAsync(pm => pm.ProjectId == project.Id && pm.UserId == newAssigneeId, ct);

            if (assigneeMember == null)
            {
                throw new InvalidOperationException("Assignee must be a member of the project");
            }
        }

        // Skip if assignee hasn't changed
        if (task.AssigneeId == newAssigneeId)
        {
            return BuildResponse(task);
        }

        var previousAssigneeId = task.AssigneeId;
        var now = DateTime.UtcNow;

        // Update task assignee
        task.AssigneeId = newAssigneeId;
        task.UpdatedTimestamp = now;

        // Record history entry for assignment change
        var historyEntry = new TaskHistory
        {
            TaskId = taskId,
            ChangeType = TaskHistoryChangeType.AssigneeChanged,
            OldValue = previousAssigneeId,
            NewValue = newAssigneeId,
            ChangedBy = userId,
            ChangedTimestamp = now
        };

        _context.TaskHistory.Add(historyEntry);
        await _taskRepository.UpdateAsync(task, ct);

        _logger.LogInformation(
            "Task {TaskId} assigned from {PreviousAssignee} to {NewAssignee} by user {UserId}",
            taskId, previousAssigneeId ?? "unassigned", newAssigneeId ?? "unassigned", userId);

        // Create notification for new assignee
        if (!string.IsNullOrEmpty(newAssigneeId) && newAssigneeId != previousAssigneeId)
        {
            var notification = new Notification
            {
                RecipientId = newAssigneeId,
                TaskId = taskId,
                Type = NotificationType.TaskAssigned,
                Content = $"You have been assigned to task: {task.Title}",
                CreatedTimestamp = now,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Notification created for user {UserId} about task assignment", newAssigneeId);
        }

        // Broadcast assignment to all project members via SignalR
        await _hubContext.Clients.Group($"project-{project.Id}").SendAsync("TaskAssigned", new
        {
            id = task.Id,
            projectId = task.ProjectId,
            title = task.Title,
            previousAssigneeId,
            newAssigneeId,
            assignedBy = userId,
            updatedTimestamp = task.UpdatedTimestamp
        }, ct);

        _logger.LogDebug("Task {TaskId} assignment broadcasted to project {ProjectId} members", taskId, project.Id);

        return BuildResponse(task);
    }

    private AssignTaskResponse BuildResponse(DomainTask task)
    {
        return new AssignTaskResponse
        {
            Id = task.Id,
            Title = task.Title,
            AssigneeId = task.AssigneeId,
            AssigneeName = task.Assignee != null 
                ? $"{task.Assignee.FirstName} {task.Assignee.LastName}".Trim()
                : null,
            UpdatedTimestamp = task.UpdatedTimestamp
        };
    }
}
