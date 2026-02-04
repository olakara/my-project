namespace TaskManagement.Api.Features.Tasks.GetTask;

public class CommentResponse
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public UserSummaryResponse Author { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? EditedAt { get; set; }
}
