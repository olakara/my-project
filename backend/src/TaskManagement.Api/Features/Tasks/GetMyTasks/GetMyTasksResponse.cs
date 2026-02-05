using TaskManagement.Api.Domain.Tasks;
using TaskManagement.Api.Features.Tasks.GetTask;
using DomainTaskStatus = TaskManagement.Api.Domain.Tasks.TaskStatus;

namespace TaskManagement.Api.Features.Tasks.GetMyTasks;

public class GetMyTasksResponse
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DomainTaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public UserSummaryResponse? Assignee { get; set; }
    public UserSummaryResponse CreatedBy { get; set; } = new();
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int CommentCount { get; set; }
    public bool IsOverdue { get; set; }
}
