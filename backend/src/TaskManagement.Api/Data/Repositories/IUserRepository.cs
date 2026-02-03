using TaskManagement.Api.Domain.Users;

namespace TaskManagement.Api.Data.Repositories;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<List<ApplicationUser>> GetByProjectAsync(int projectId, CancellationToken ct = default);
    Task<ApplicationUser> CreateAsync(ApplicationUser user, CancellationToken ct = default);
    Task UpdateAsync(ApplicationUser user, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
}
