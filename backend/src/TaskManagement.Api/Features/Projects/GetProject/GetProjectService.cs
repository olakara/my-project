using TaskManagement.Api.Data.Repositories;
using TaskManagement.Api.Domain.Projects;

namespace TaskManagement.Api.Features.Projects.GetProject;

public interface IGetProjectService
{
    System.Threading.Tasks.Task<GetProjectResponse> GetProjectAsync(int projectId, string userId, CancellationToken ct = default);
}

public class GetProjectService : IGetProjectService
{
    private readonly IProjectRepository _projectRepository;

    public GetProjectService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async System.Threading.Tasks.Task<GetProjectResponse> GetProjectAsync(int projectId, string userId, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, ct);
        if (project == null)
        {
            throw new KeyNotFoundException("Project not found");
        }

        var isMember = project.OwnerId == userId || project.Members.Any(m => m.UserId == userId);
        if (!isMember)
        {
            throw new UnauthorizedAccessException("User is not a member of this project");
        }

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

    private static ProjectRole ResolveRole(Project project, string userId)
    {
        if (project.OwnerId == userId)
        {
            return ProjectRole.Owner;
        }

        return project.GetUserRole(userId) ?? ProjectRole.Member;
    }
}
