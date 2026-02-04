using TaskManagement.Api.Domain.Tasks;

namespace TaskManagement.Api.Features.Tasks.GetTask;

public class GetTaskResponse
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public UserSummaryResponse? Assignee { get; set; }
    public UserSummaryResponse CreatedBy { get; set; } = new();
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int CommentCount { get; set; }
    public bool IsOverdue { get; set; }
    public List<CommentResponse> Comments { get; set; } = new();
    public List<TaskHistoryResponse> HistoryPreview { get; set; } = new();
}
