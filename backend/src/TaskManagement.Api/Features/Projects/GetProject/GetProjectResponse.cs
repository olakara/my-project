namespace TaskManagement.Api.Features.Projects.GetProject;

public class GetProjectResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Role { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public int TaskCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public ProjectOwnerResponse Owner { get; set; } = new();
    public List<ProjectMemberResponse> Members { get; set; } = new();
}
