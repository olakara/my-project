using TaskManagement.Api.Hubs.Events;

namespace TaskManagement.Api.Services;

/// <summary>
/// Service for broadcasting real-time notifications via SignalR.
/// Services inject this interface to send events to connected clients.
/// </summary>
public interface ISignalRNotificationService
{
    /// <summary>
    /// Broadcasts task created event to project members.
    /// </summary>
    Task NotifyTaskCreatedAsync(int projectId, TaskCreatedEvent eventData);

    /// <summary>
    /// Broadcasts task updated event to project members.
    /// </summary>
    Task NotifyTaskUpdatedAsync(int projectId, TaskUpdatedEvent eventData);

    /// <summary>
    /// Broadcasts task status changed event to project members.
    /// </summary>
    Task NotifyTaskStatusChangedAsync(int projectId, TaskStatusChangedEvent eventData);

    /// <summary>
    /// Broadcasts task assigned event to project members.
    /// </summary>
    Task NotifyTaskAssignedAsync(int projectId, TaskAssignedEvent eventData);

    /// <summary>
    /// Broadcasts task deleted event to project members.
    /// </summary>
    Task NotifyTaskDeletedAsync(int projectId, TaskDeletedEvent eventData);

    /// <summary>
    /// Broadcasts comment added event to project members.
    /// </summary>
    Task NotifyCommentAddedAsync(int projectId, CommentAddedEvent eventData);

    /// <summary>
    /// Broadcasts project member joined event to project members.
    /// </summary>
    Task NotifyProjectMemberJoinedAsync(int projectId, ProjectMemberJoinedEvent eventData);

    /// <summary>
    /// Broadcasts project member left event to project members.
    /// </summary>
    Task NotifyProjectMemberLeftAsync(int projectId, ProjectMemberLeftEvent eventData);
}
