using TaskManagement.Api.Domain.Users;
using TaskManagement.Api.Domain.Projects;
using TaskManagement.Api.Domain.Notifications;

namespace TaskManagement.Api.Domain.Tasks;

public class Task
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.ToDo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public int ProjectId { get; set; }
    public string? AssigneeId { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedTimestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Project Project { get; set; } = null!;
    public ApplicationUser? Assignee { get; set; }
    public ApplicationUser Creator { get; set; } = null!;
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<TaskHistory> History { get; set; } = new List<TaskHistory>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    
    // Business methods
    public bool CanEdit(string userId) => CreatedBy == userId;
    public bool IsOverdue => DueDate.HasValue && DueDate < DateTime.UtcNow && Status != TaskStatus.Done;
}

public enum TaskStatus
{
    ToDo,
    InProgress,
    InReview,
    Done
}

public enum TaskPriority
{
    Low,
    Medium,
    High
}
