using TaskManagement.Api.Domain.Tasks;
using DomainTaskStatus = TaskManagement.Api.Domain.Tasks.TaskStatus;

namespace TaskManagement.Api.Features.Tasks.UpdateTaskStatus;

public class UpdateTaskStatusRequest
{
    public DomainTaskStatus NewStatus { get; set; }
}

public class UpdateTaskStatusResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DomainTaskStatus Status { get; set; }
    public string? AssigneeName { get; set; }
    public DateTime UpdatedTimestamp { get; set; }
}
