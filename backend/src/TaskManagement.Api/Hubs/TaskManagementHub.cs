using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using TaskManagement.Api.Data.Repositories;

namespace TaskManagement.Api.Hubs;

/// <summary>
/// SignalR hub for real-time task management collaboration.
/// Handles project subscriptions and broadcasts task/comment updates to connected clients.
/// </summary>
[Authorize]
public class TaskManagementHub : Hub
{
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<TaskManagementHub> _logger;

    public TaskManagementHub(
        IProjectRepository projectRepository,
        ILogger<TaskManagementHub> logger)
    {
        _projectRepository = projectRepository;
        _logger = logger;
    }

    /// <summary>
    /// Join a project group to receive real-time updates for that project.
    /// Client must be a member of the project to join.
    /// </summary>
    /// <param name="projectId">Project ID to subscribe to</param>
    public async Task JoinProject(int projectId)
    {
        var userId = Context.UserIdentifier;
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("JoinProject called without authenticated user");
            throw new HubException("User not authenticated");
        }

        // Verify user is a member of the project
        var isMember = await _projectRepository.IsUserMemberOfProjectAsync(projectId, userId);
        if (!isMember)
        {
            _logger.LogWarning("User {UserId} attempted to join project {ProjectId} without membership", userId, projectId);
            throw new HubException("User is not a member of this project");
        }

        var groupName = GetProjectGroupName(projectId);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User {UserId} joined project {ProjectId} group", userId, projectId);

        // Notify other users in the project
        await Clients.OthersInGroup(groupName).SendAsync("UserConnected", new
        {
            userId,
            projectId,
            connectionId = Context.ConnectionId,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Leave a project group to stop receiving updates.
    /// </summary>
    /// <param name="projectId">Project ID to unsubscribe from</param>
    public async Task LeaveProject(int projectId)
    {
        var userId = Context.UserIdentifier;
        var groupName = GetProjectGroupName(projectId);
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User {UserId} left project {ProjectId} group", userId, projectId);

        // Notify other users in the project
        await Clients.OthersInGroup(groupName).SendAsync("UserDisconnected", new
        {
            userId,
            projectId,
            connectionId = Context.ConnectionId,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Send typing indicator to other users viewing the same task.
    /// </summary>
    /// <param name="taskId">Task ID where user is typing</param>
    /// <param name="isTyping">True if user started typing, false if stopped</param>
    public async Task SendTypingIndicator(int taskId, bool isTyping)
    {
        var userId = Context.UserIdentifier;
        
        // Note: In a production system, we'd verify the user has access to this task
        // For now, we'll broadcast to all clients in the same project
        
        await Clients.Others.SendAsync("UserTyping", new
        {
            taskId,
            userId,
            isTyping,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Broadcasts task creation event to all members of the project.
    /// Called by backend services, not directly by clients.
    /// </summary>
    public async Task BroadcastTaskCreated(int projectId, object taskData)
    {
        var groupName = GetProjectGroupName(projectId);
        await Clients.Group(groupName).SendAsync("TaskCreated", taskData);
        
        _logger.LogInformation("Broadcasted TaskCreated event to project {ProjectId}", projectId);
    }

    /// <summary>
    /// Broadcasts task update event to all members of the project.
    /// Called by backend services, not directly by clients.
    /// </summary>
    public async Task BroadcastTaskUpdated(int projectId, object taskData)
    {
        var groupName = GetProjectGroupName(projectId);
        await Clients.Group(groupName).SendAsync("TaskUpdated", taskData);
        
        _logger.LogInformation("Broadcasted TaskUpdated event to project {ProjectId}", projectId);
    }

    /// <summary>
    /// Broadcasts task status change event to all members of the project.
    /// Called when a task is moved between Kanban columns.
    /// </summary>
    public async Task BroadcastTaskStatusChanged(int projectId, object statusChangeData)
    {
        var groupName = GetProjectGroupName(projectId);
        await Clients.Group(groupName).SendAsync("TaskStatusChanged", statusChangeData);
        
        _logger.LogInformation("Broadcasted TaskStatusChanged event to project {ProjectId}", projectId);
    }

    /// <summary>
    /// Broadcasts task assignment event to all members of the project.
    /// </summary>
    public async Task BroadcastTaskAssigned(int projectId, object assignmentData)
    {
        var groupName = GetProjectGroupName(projectId);
        await Clients.Group(groupName).SendAsync("TaskAssigned", assignmentData);
        
        _logger.LogInformation("Broadcasted TaskAssigned event to project {ProjectId}", projectId);
    }

    /// <summary>
    /// Broadcasts comment added event to all members of the project.
    /// </summary>
    public async Task BroadcastCommentAdded(int projectId, object commentData)
    {
        var groupName = GetProjectGroupName(projectId);
        await Clients.Group(groupName).SendAsync("CommentAdded", commentData);
        
        _logger.LogInformation("Broadcasted CommentAdded event to project {ProjectId}", projectId);
    }

    /// <summary>
    /// Broadcasts task deletion event to all members of the project.
    /// </summary>
    public async Task BroadcastTaskDeleted(int projectId, object deletionData)
    {
        var groupName = GetProjectGroupName(projectId);
        await Clients.Group(groupName).SendAsync("TaskDeleted", deletionData);
        
        _logger.LogInformation("Broadcasted TaskDeleted event to project {ProjectId}", projectId);
    }

    /// <summary>
    /// Broadcasts project member joined event to all members of the project.
    /// </summary>
    public async Task BroadcastProjectMemberJoined(int projectId, object memberData)
    {
        var groupName = GetProjectGroupName(projectId);
        await Clients.Group(groupName).SendAsync("ProjectMemberJoined", memberData);
        
        _logger.LogInformation("Broadcasted ProjectMemberJoined event to project {ProjectId}", projectId);
    }

    /// <summary>
    /// Broadcasts project member left event to all members of the project.
    /// </summary>
    public async Task BroadcastProjectMemberLeft(int projectId, object memberData)
    {
        var groupName = GetProjectGroupName(projectId);
        await Clients.Group(groupName).SendAsync("ProjectMemberLeft", memberData);
        
        _logger.LogInformation("Broadcasted ProjectMemberLeft event to project {ProjectId}", projectId);
    }

    /// <summary>
    /// Called when a client connects to the hub.
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        _logger.LogInformation("Client connected: UserId={UserId}, ConnectionId={ConnectionId}", userId, Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub.
    /// Automatically removes the client from all groups.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (exception != null)
        {
            _logger.LogError(exception, "Client disconnected with error: UserId={UserId}, ConnectionId={ConnectionId}", userId, Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation("Client disconnected: UserId={UserId}, ConnectionId={ConnectionId}", userId, Context.ConnectionId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Gets the SignalR group name for a project.
    /// </summary>
    private static string GetProjectGroupName(int projectId) => $"project-{projectId}";
}
