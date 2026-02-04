using TaskManagement.Api.Domain.Tasks;

namespace TaskManagement.Api.Features.Tasks.CreateTask;

public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AssigneeId { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
}
