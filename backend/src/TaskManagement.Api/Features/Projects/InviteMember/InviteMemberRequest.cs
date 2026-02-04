using TaskManagement.Api.Domain.Projects;

namespace TaskManagement.Api.Features.Projects.InviteMember;

public class InviteMemberRequest
{
    public string Email { get; set; } = string.Empty;
    public ProjectRole Role { get; set; }
}
