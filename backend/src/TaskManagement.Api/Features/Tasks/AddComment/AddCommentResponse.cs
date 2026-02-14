namespace TaskManagement.Api.Features.Tasks.AddComment;

public class AddCommentResponse
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedTimestamp { get; set; }
    public DateTime? EditedTimestamp { get; set; }
}
