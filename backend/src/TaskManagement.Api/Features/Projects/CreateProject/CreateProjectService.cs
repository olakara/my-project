using TaskManagement.Api.Data;
using TaskManagement.Api.Data.Repositories;
using TaskManagement.Api.Domain.Projects;
using TaskManagement.Api.Domain.Users;

namespace TaskManagement.Api.Features.Projects.CreateProject;

public interface ICreateProjectService
{
    System.Threading.Tasks.Task<CreateProjectResponse> CreateProjectAsync(string userId, CreateProjectRequest request, CancellationToken ct = default);
}

public class CreateProjectService : ICreateProjectService
{
    private readonly TaskManagementDbContext _context;
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<CreateProjectService> _logger;

    public CreateProjectService(TaskManagementDbContext context, IProjectRepository projectRepository, ILogger<CreateProjectService> logger)
    {
        _context = context;
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<CreateProjectResponse> CreateProjectAsync(string userId, CreateProjectRequest request, CancellationToken ct = default)
    {
        var project = new Project
        {
            Name = request.Name,
            Description = request.Description,
            OwnerId = userId,
            IsArchived = false,
            CreatedTimestamp = DateTime.UtcNow,
            UpdatedTimestamp = DateTime.UtcNow
        };

        // Create the project
        var createdProject = await _projectRepository.CreateAsync(project, ct);

        // Add creator as Owner member
        var projectMember = new ProjectMember
        {
            UserId = userId,
            ProjectId = createdProject.Id,
            Role = ProjectRole.Owner,
            JoinedTimestamp = DateTime.UtcNow
        };

        _context.ProjectMembers.Add(projectMember);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Project {ProjectId} created by user {UserId} with name '{ProjectName}'", 
            createdProject.Id, userId, createdProject.Name);

        return new CreateProjectResponse
        {
            Id = createdProject.Id,
            Name = createdProject.Name,
            Description = createdProject.Description,
            OwnerId = createdProject.OwnerId,
            IsArchived = createdProject.IsArchived,
            CreatedTimestamp = createdProject.CreatedTimestamp,
            UpdatedTimestamp = createdProject.UpdatedTimestamp
        };
    }
}
