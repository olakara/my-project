using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Domain.Users;

namespace TaskManagement.Api.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly TaskManagementDbContext _context;

    public UserRepository(TaskManagementDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicationUser?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return await _context.Users
            .Include(u => u.ProjectMemberships)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }

    public async Task<List<ApplicationUser>> GetByProjectAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.Users
            .Where(u => u.ProjectMemberships.Any(pm => pm.ProjectId == projectId))
            .ToListAsync(ct);
    }

    public async Task<ApplicationUser> CreateAsync(ApplicationUser user, CancellationToken ct = default)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);
        return user;
    }

    public async Task UpdateAsync(ApplicationUser user, CancellationToken ct = default)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        var user = await GetByIdAsync(id, ct);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync(ct);
        }
    }
}
