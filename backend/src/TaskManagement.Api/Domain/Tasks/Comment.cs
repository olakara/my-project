using TaskManagement.Api.Domain.Users;

namespace TaskManagement.Api.Domain.Tasks;

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public int TaskId { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;
    public DateTime? EditedTimestamp { get; set; }
    
    // Navigation properties
    public Task Task { get; set; } = null!;
    public ApplicationUser Author { get; set; } = null!;
}
