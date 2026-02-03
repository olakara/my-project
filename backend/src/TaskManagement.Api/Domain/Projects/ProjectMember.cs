using TaskManagement.Api.Domain.Projects;

namespace TaskManagement.Api.Domain.Users;

public class ProjectMember
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public ProjectRole Role { get; set; }
    public DateTime JoinedTimestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public Project Project { get; set; } = null!;
    
    // Business methods
    public bool CanManageProject() => Role == ProjectRole.Owner || Role == ProjectRole.Manager;
    public bool CanDeleteProject() => Role == ProjectRole.Owner;
}
