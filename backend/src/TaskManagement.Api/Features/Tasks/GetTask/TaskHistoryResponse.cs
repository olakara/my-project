using TaskManagement.Api.Domain.Tasks;

namespace TaskManagement.Api.Features.Tasks.GetTask;

public class TaskHistoryResponse
{
    public int Id { get; set; }
    public TaskHistoryChangeType ChangeType { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public UserSummaryResponse ChangedBy { get; set; } = new();
    public DateTime ChangedAt { get; set; }
}
