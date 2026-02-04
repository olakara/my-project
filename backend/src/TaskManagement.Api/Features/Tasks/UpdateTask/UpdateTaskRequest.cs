using TaskManagement.Api.Domain.Tasks;

namespace TaskManagement.Api.Features.Tasks.UpdateTask;

public class UpdateTaskRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public TaskPriority? Priority { get; set; }
    public DateTime? DueDate { get; set; }
}
