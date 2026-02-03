using DomainTask = TaskManagement.Api.Domain.Tasks.Task;
using DomainTaskStatus = TaskManagement.Api.Domain.Tasks.TaskStatus;

namespace TaskManagement.Api.Data.Repositories;

public interface ITaskRepository
{
    System.Threading.Tasks.Task<DomainTask?> GetByIdAsync(int id, CancellationToken ct = default);
    System.Threading.Tasks.Task<List<DomainTask>> GetByProjectAsync(int projectId, CancellationToken ct = default);
    System.Threading.Tasks.Task<List<DomainTask>> GetByAssigneeAsync(string userId, CancellationToken ct = default);
    System.Threading.Tasks.Task<List<DomainTask>> GetByProjectAndStatusAsync(int projectId, DomainTaskStatus status, CancellationToken ct = default);
    System.Threading.Tasks.Task<DomainTask> CreateAsync(DomainTask task, CancellationToken ct = default);
    System.Threading.Tasks.Task UpdateAsync(DomainTask task, CancellationToken ct = default);
    System.Threading.Tasks.Task DeleteAsync(int id, CancellationToken ct = default);
}
