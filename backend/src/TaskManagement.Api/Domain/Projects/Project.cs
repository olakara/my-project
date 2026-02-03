using TaskManagement.Api.Domain.Users;
using DomainTask = TaskManagement.Api.Domain.Tasks.Task;

namespace TaskManagement.Api.Domain.Projects;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public bool IsArchived { get; set; } = false;
    public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedTimestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ApplicationUser Owner { get; set; } = null!;
    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public ICollection<DomainTask> Tasks { get; set; } = new List<DomainTask>();
    public ICollection<ProjectInvitation> Invitations { get; set; } = new List<ProjectInvitation>();
    
    // Business methods
    public bool HasMember(string userId) => Members.Any(m => m.UserId == userId);
    public bool IsOwner(string userId) => OwnerId == userId;
    public ProjectRole? GetUserRole(string userId) => Members.FirstOrDefault(m => m.UserId == userId)?.Role;
    public bool CanManageMembers(string userId) => IsOwner(userId) || GetUserRole(userId) == ProjectRole.Manager;
}

public enum ProjectRole
{
    Owner,
    Manager,
    Member
}
