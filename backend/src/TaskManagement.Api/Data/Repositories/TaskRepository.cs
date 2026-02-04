using Microsoft.EntityFrameworkCore;
using DomainTask = TaskManagement.Api.Domain.Tasks.Task;
using DomainTaskStatus = TaskManagement.Api.Domain.Tasks.TaskStatus;

namespace TaskManagement.Api.Data.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly TaskManagementDbContext _context;

    public TaskRepository(TaskManagementDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task<DomainTask?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.Tasks
            .Include(t => t.Assignee)
            .Include(t => t.Creator)
            .Include(t => t.Comments)
                .ThenInclude(c => c.Author)
            .Include(t => t.History)
                .ThenInclude(h => h.ChangedByUser)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async System.Threading.Tasks.Task<List<DomainTask>> GetByProjectAsync(int projectId, CancellationToken ct = default)
    {
        return await _context.Tasks
            .Where(t => t.ProjectId == projectId)
            .Include(t => t.Assignee)
            .Include(t => t.Creator)
            .OrderByDescending(t => t.CreatedTimestamp)
            .ToListAsync(ct);
    }

    public async System.Threading.Tasks.Task<List<DomainTask>> GetByAssigneeAsync(string userId, CancellationToken ct = default)
    {
        return await _context.Tasks
            .Where(t => t.AssigneeId == userId)
            .Include(t => t.Project)
            .Include(t => t.Creator)
            .OrderByDescending(t => t.CreatedTimestamp)
            .ToListAsync(ct);
    }

    public async System.Threading.Tasks.Task<List<DomainTask>> GetByProjectAndStatusAsync(int projectId, DomainTaskStatus status, CancellationToken ct = default)
    {
        return await _context.Tasks
            .Where(t => t.ProjectId == projectId && t.Status == status)
            .Include(t => t.Assignee)
            .Include(t => t.Creator)
            .OrderByDescending(t => t.CreatedTimestamp)
            .ToListAsync(ct);
    }

    public async System.Threading.Tasks.Task<DomainTask> CreateAsync(DomainTask task, CancellationToken ct = default)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync(ct);
        return task;
    }

    public async System.Threading.Tasks.Task UpdateAsync(DomainTask task, CancellationToken ct = default)
    {
        _context.Tasks.Update(task);
        await _context.SaveChangesAsync(ct);
    }

    public async System.Threading.Tasks.Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var task = await GetByIdAsync(id, ct);
        if (task is not null)
        {
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync(ct);
        }
    }
}
