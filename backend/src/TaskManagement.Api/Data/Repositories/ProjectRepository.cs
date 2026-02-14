using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Domain.Projects;

namespace TaskManagement.Api.Data.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly TaskManagementDbContext _context;

    public ProjectRepository(TaskManagementDbContext context)
    {
        _context = context;
    }

    public async Task<Project?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Projects
            .Include(p => p.Owner)
            .Include(p => p.Members)
                .ThenInclude(m => m.User)
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<List<Project>> GetByUserAsync(string userId, CancellationToken ct = default)
    {
        return await _context.Projects
            .Where(p => p.Members.Any(m => m.UserId == userId) || p.OwnerId == userId)
            .Include(p => p.Owner)
            .Include(p => p.Members)
            .Include(p => p.Tasks)
            .OrderByDescending(p => p.CreatedTimestamp)
            .ToListAsync(ct);
    }

    public async Task<List<Project>> GetAllAsync(CancellationToken ct = default)
    {
        return await _context.Projects
            .Include(p => p.Owner)
            .Include(p => p.Members)
            .OrderByDescending(p => p.CreatedTimestamp)
            .ToListAsync(ct);
    }

    public async Task<Project> CreateAsync(Project project, CancellationToken ct = default)
    {
        _context.Projects.Add(project);
        await _context.SaveChangesAsync(ct);
        return project;
    }

    public async Task UpdateAsync(Project project, CancellationToken ct = default)
    {
        _context.Projects.Update(project);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var project = await GetByIdAsync(id, ct);
        if (project != null)
        {
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> IsUserMemberOfProjectAsync(int projectId, string userId, CancellationToken ct = default)
    {
        return await _context.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId, ct);
    }
}
