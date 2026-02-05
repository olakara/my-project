namespace TaskManagement.Api.Features.Tasks.AssignTask;

public class AssignTaskRequest
{
    public string? AssigneeId { get; set; }
}

public class AssignTaskResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? AssigneeId { get; set; }
    public string? AssigneeName { get; set; }
    public DateTime UpdatedTimestamp { get; set; }
}
