using TaskManagement.Api.Domain.Projects;

namespace TaskManagement.Api.Data.Repositories;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Project>> GetByUserAsync(string userId, CancellationToken ct = default);
    Task<List<Project>> GetAllAsync(CancellationToken ct = default);
    Task<Project> CreateAsync(Project project, CancellationToken ct = default);
    Task UpdateAsync(Project project, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
    Task<bool> IsUserMemberOfProjectAsync(int projectId, string userId, CancellationToken ct = default);
}
