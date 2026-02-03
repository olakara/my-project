using Microsoft.AspNetCore.Identity;
using TaskManagement.Api.Domain.Projects;
using DomainTask = TaskManagement.Api.Domain.Tasks.Task;
using TaskManagement.Api.Domain.Notifications;
using TaskManagement.Api.Domain.Tasks;

namespace TaskManagement.Api.Domain.Users;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public DateTime CreatedTimestamp { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedTimestamp { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();
    public ICollection<DomainTask> AssignedTasks { get; set; } = new List<DomainTask>();
    public ICollection<DomainTask> CreatedTasks { get; set; } = new List<DomainTask>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<ProjectInvitation> SentInvitations { get; set; } = new List<ProjectInvitation>();
    public string FullName => $"{FirstName} {LastName}".Trim();
}
