using Microsoft.AspNetCore.SignalR;
using TaskManagement.Api.Hubs;
using TaskManagement.Api.Hubs.Events;

namespace TaskManagement.Api.Services;

/// <summary>
/// Implementation of SignalR notification service.
/// Broadcasts real-time events to connected clients via TaskManagementHub.
/// </summary>
public class SignalRNotificationService : ISignalRNotificationService
{
    private readonly IHubContext<TaskManagementHub> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(
        IHubContext<TaskManagementHub> hubContext,
        ILogger<SignalRNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyTaskCreatedAsync(int projectId, TaskCreatedEvent eventData)
    {
        try
        {
            var groupName = GetProjectGroupName(projectId);
            await _hubContext.Clients.Group(groupName).SendAsync("TaskCreated", eventData);
            _logger.LogInformation("Notified TaskCreated event to project {ProjectId}", projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify TaskCreated event to project {ProjectId}", projectId);
        }
    }

    public async Task NotifyTaskUpdatedAsync(int projectId, TaskUpdatedEvent eventData)
    {
        try
        {
            var groupName = GetProjectGroupName(projectId);
            await _hubContext.Clients.Group(groupName).SendAsync("TaskUpdated", eventData);
            _logger.LogInformation("Notified TaskUpdated event to project {ProjectId}", projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify TaskUpdated event to project {ProjectId}", projectId);
        }
    }

    public async Task NotifyTaskStatusChangedAsync(int projectId, TaskStatusChangedEvent eventData)
    {
        try
        {
            var groupName = GetProjectGroupName(projectId);
            await _hubContext.Clients.Group(groupName).SendAsync("TaskStatusChanged", eventData);
            _logger.LogInformation("Notified TaskStatusChanged event to project {ProjectId}", projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify TaskStatusChanged event to project {ProjectId}", projectId);
        }
    }

    public async Task NotifyTaskAssignedAsync(int projectId, TaskAssignedEvent eventData)
    {
        try
        {
            var groupName = GetProjectGroupName(projectId);
            await _hubContext.Clients.Group(groupName).SendAsync("TaskAssigned", eventData);
            _logger.LogInformation("Notified TaskAssigned event to project {ProjectId}", projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify TaskAssigned event to project {ProjectId}", projectId);
        }
    }

    public async Task NotifyTaskDeletedAsync(int projectId, TaskDeletedEvent eventData)
    {
        try
        {
            var groupName = GetProjectGroupName(projectId);
            await _hubContext.Clients.Group(groupName).SendAsync("TaskDeleted", eventData);
            _logger.LogInformation("Notified TaskDeleted event to project {ProjectId}", projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify TaskDeleted event to project {ProjectId}", projectId);
        }
    }

    public async Task NotifyCommentAddedAsync(int projectId, CommentAddedEvent eventData)
    {
        try
        {
            var groupName = GetProjectGroupName(projectId);
            await _hubContext.Clients.Group(groupName).SendAsync("CommentAdded", eventData);
            _logger.LogInformation("Notified CommentAdded event to project {ProjectId}", projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify CommentAdded event to project {ProjectId}", projectId);
        }
    }

    public async Task NotifyProjectMemberJoinedAsync(int projectId, ProjectMemberJoinedEvent eventData)
    {
        try
        {
            var groupName = GetProjectGroupName(projectId);
            await _hubContext.Clients.Group(groupName).SendAsync("ProjectMemberJoined", eventData);
            _logger.LogInformation("Notified ProjectMemberJoined event to project {ProjectId}", projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify ProjectMemberJoined event to project {ProjectId}", projectId);
        }
    }

    public async Task NotifyProjectMemberLeftAsync(int projectId, ProjectMemberLeftEvent eventData)
    {
        try
        {
            var groupName = GetProjectGroupName(projectId);
            await _hubContext.Clients.Group(groupName).SendAsync("ProjectMemberLeft", eventData);
            _logger.LogInformation("Notified ProjectMemberLeft event to project {ProjectId}", projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify ProjectMemberLeft event to project {ProjectId}", projectId);
        }
    }

    /// <summary>
    /// Gets the SignalR group name for a project.
    /// Must match the convention used in TaskManagementHub.
    /// </summary>
    private static string GetProjectGroupName(int projectId) => $"project-{projectId}";
}
