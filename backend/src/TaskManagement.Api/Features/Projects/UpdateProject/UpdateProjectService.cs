using TaskManagement.Api.Data.Repositories;
using TaskManagement.Api.Domain.Projects;
using TaskManagement.Api.Features.Projects.GetProject;

namespace TaskManagement.Api.Features.Projects.UpdateProject;

public interface IUpdateProjectService
{
    System.Threading.Tasks.Task<GetProjectResponse> UpdateProjectAsync(int projectId, string userId, UpdateProjectRequest request, CancellationToken ct = default);
}

public class UpdateProjectService : IUpdateProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<UpdateProjectService> _logger;

    public UpdateProjectService(IProjectRepository projectRepository, ILogger<UpdateProjectService> logger)
    {
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<GetProjectResponse> UpdateProjectAsync(int projectId, string userId, UpdateProjectRequest request, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, ct);
        if (project == null)
        {
            throw new KeyNotFoundException("Project not found");
        }

        if (!CanManageProject(project, userId))
        {
            throw new UnauthorizedAccessException("User is not authorized to update this project");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            project.Name = request.Name.Trim();
        }

        if (request.Description != null)
        {
            project.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        }

        project.UpdatedTimestamp = DateTime.UtcNow;

        await _projectRepository.UpdateAsync(project, ct);

        _logger.LogInformation("Project {ProjectId} updated by user {UserId}", project.Id, userId);

        return new GetProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Role = ResolveRole(project, userId).ToString(),
            MemberCount = project.Members.Count,
            TaskCount = project.Tasks.Count,
            CreatedAt = project.CreatedTimestamp,
            Owner = new ProjectOwnerResponse
            {
                UserId = project.Owner.Id,
                FullName = string.Join(" ", new[] { project.Owner.FirstName, project.Owner.LastName }.Where(n => !string.IsNullOrWhiteSpace(n))),
                Email = project.Owner.Email ?? string.Empty
            },
            Members = project.Members
                .Select(member => new ProjectMemberResponse
                {
                    UserId = member.UserId,
                    FullName = string.Join(" ", new[] { member.User.FirstName, member.User.LastName }.Where(n => !string.IsNullOrWhiteSpace(n))),
                    Email = member.User.Email ?? string.Empty,
                    Role = member.Role.ToString(),
                    JoinedAt = member.JoinedTimestamp
                })
                .OrderBy(m => m.Role)
                .ThenBy(m => m.FullName)
                .ToList()
        };
    }

    private static bool CanManageProject(Project project, string userId)
    {
        if (project.OwnerId == userId)
        {
            return true;
        }

        var role = project.GetUserRole(userId);
        return role == ProjectRole.Manager;
    }

    private static ProjectRole ResolveRole(Project project, string userId)
    {
        if (project.OwnerId == userId)
        {
            return ProjectRole.Owner;
        }

        return project.GetUserRole(userId) ?? ProjectRole.Member;
    }
}
