using TaskManagement.Api.Domain.Users;

namespace TaskManagement.Api.Domain.Projects;

public class ProjectInvitation
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public string InviterId { get; set; } = string.Empty;
    public ProjectRole Role { get; set; }
    public ProjectInvitationStatus Status { get; set; } = ProjectInvitationStatus.Pending;
    public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresTimestamp { get; set; }
    
    // Navigation properties
    public Project Project { get; set; } = null!;
    public ApplicationUser Inviter { get; set; } = null!;
    
    public bool IsExpired => DateTime.UtcNow > ExpiresTimestamp;
}

public enum ProjectInvitationStatus
{
    Pending,
    Accepted,
    Declined
}
