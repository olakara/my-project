namespace TaskManagement.Api.Features.Projects.CreateProject;

public class CreateProjectRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
