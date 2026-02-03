using TaskManagement.Api.Domain.Users;
using DomainTask = TaskManagement.Api.Domain.Tasks.Task;

namespace TaskManagement.Api.Domain.Notifications;

public class Notification
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public int? TaskId { get; set; }
    public string RecipientId { get; set; } = string.Empty;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;
    public DateTime? ReadTimestamp { get; set; }
    
    // Navigation properties
    public ApplicationUser Recipient { get; set; } = null!;
    public DomainTask? Task { get; set; }
}

public enum NotificationType
{
    TaskAssigned,
    StatusChanged,
    CommentAdded,
    MemberInvited
}
