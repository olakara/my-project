using TaskManagement.Api.Data.Repositories;
using TaskManagement.Api.Domain.Projects;

namespace TaskManagement.Api.Features.Projects.GetProjects;

public interface IGetProjectsService
{
    System.Threading.Tasks.Task<IReadOnlyList<ProjectSummaryResponse>> GetProjectsAsync(string userId, CancellationToken ct = default);
}

public class GetProjectsService : IGetProjectsService
{
    private readonly IProjectRepository _projectRepository;

    public GetProjectsService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async System.Threading.Tasks.Task<IReadOnlyList<ProjectSummaryResponse>> GetProjectsAsync(string userId, CancellationToken ct = default)
    {
        var projects = await _projectRepository.GetByUserAsync(userId, ct);

        return projects
            .Select(project => new ProjectSummaryResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Role = ResolveRole(project, userId).ToString(),
                MemberCount = project.Members.Count,
                TaskCount = project.Tasks.Count,
                CreatedAt = project.CreatedTimestamp
            })
            .ToList();
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
