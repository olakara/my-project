namespace TaskManagement.Api.Hubs.Events;

/// <summary>
/// Event data for task created notification.
/// </summary>
public class TaskCreatedEvent
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public UserSummary? Assignee { get; set; }
    public DateTime? DueDate { get; set; }
    public int ProjectId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Event data for task updated notification.
/// </summary>
public class TaskUpdatedEvent
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public UserSummary? Assignee { get; set; }
    public DateTime? DueDate { get; set; }
    public int ProjectId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Event data for task status changed notification (Kanban drag-drop).
/// </summary>
public class TaskStatusChangedEvent
{
    public int TaskId { get; set; }
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public UserSummary UpdatedBy { get; set; } = null!;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Event data for task assignment notification.
/// </summary>
public class TaskAssignedEvent
{
    public int TaskId { get; set; }
    public UserSummary Assignee { get; set; } = null!;
    public UserSummary AssignedBy { get; set; } = null!;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Event data for task deletion notification.
/// </summary>
public class TaskDeletedEvent
{
    public int TaskId { get; set; }
    public UserSummary DeletedBy { get; set; } = null!;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Event data for comment added notification.
/// </summary>
public class CommentAddedEvent
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public int TaskId { get; set; }
    public UserSummary Author { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Event data for user typing indicator.
/// </summary>
public class UserTypingEvent
{
    public int TaskId { get; set; }
    public UserSummary User { get; set; } = null!;
    public bool IsTyping { get; set; }
}

/// <summary>
/// Event data for project member joined notification.
/// </summary>
public class ProjectMemberJoinedEvent
{
    public int ProjectId { get; set; }
    public ProjectMemberInfo Member { get; set; } = null!;
}

/// <summary>
/// Event data for project member left notification.
/// </summary>
public class ProjectMemberLeftEvent
{
    public int ProjectId { get; set; }
    public string UserId { get; set; } = string.Empty;
}

/// <summary>
/// User summary information for events.
/// </summary>
public class UserSummary
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
}

/// <summary>
/// Project member information for events.
/// </summary>
public class ProjectMemberInfo
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}
