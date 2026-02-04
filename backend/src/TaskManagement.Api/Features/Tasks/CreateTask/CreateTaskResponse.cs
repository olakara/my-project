using TaskManagement.Api.Domain.Tasks;

namespace TaskManagement.Api.Features.Tasks.CreateTask;

public class CreateTaskResponse
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public string? AssigneeId { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedTimestamp { get; set; }
    public DateTime UpdatedTimestamp { get; set; }
}
