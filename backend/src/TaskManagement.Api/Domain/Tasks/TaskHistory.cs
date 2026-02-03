using TaskManagement.Api.Domain.Users;

namespace TaskManagement.Api.Domain.Tasks;

public class TaskHistory
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public TaskHistoryChangeType ChangeType { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime ChangedTimestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public Task Task { get; set; } = null!;
    public ApplicationUser ChangedByUser { get; set; } = null!;
}

public enum TaskHistoryChangeType
{
    StatusChanged,
    AssigneeChanged,
    TitleChanged,
    DescriptionChanged,
    PriorityChanged,
    DueDateChanged
}
