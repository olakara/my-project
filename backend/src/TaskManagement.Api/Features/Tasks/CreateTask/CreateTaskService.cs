using TaskManagement.Api.Data;
using TaskManagement.Api.Data.Repositories;
using TaskManagement.Api.Domain.Tasks;
using TaskManagement.Api.Domain.Projects;
using DomainTask = TaskManagement.Api.Domain.Tasks.Task;
using DomainTaskStatus = TaskManagement.Api.Domain.Tasks.TaskStatus;

namespace TaskManagement.Api.Features.Tasks.CreateTask;

public interface ICreateTaskService
{
    System.Threading.Tasks.Task<CreateTaskResponse> CreateTaskAsync(int projectId, string userId, CreateTaskRequest request, CancellationToken ct = default);
}

public class CreateTaskService : ICreateTaskService
{
    private readonly TaskManagementDbContext _context;
    private readonly IProjectRepository _projectRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ILogger<CreateTaskService> _logger;

    public CreateTaskService(
        TaskManagementDbContext context,
        IProjectRepository projectRepository,
        ITaskRepository taskRepository,
        ILogger<CreateTaskService> logger)
    {
        _context = context;
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<CreateTaskResponse> CreateTaskAsync(int projectId, string userId, CreateTaskRequest request, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, ct);
        if (project == null)
        {
            throw new KeyNotFoundException("Project not found");
        }

        if (!IsProjectMember(project, userId))
        {
            throw new UnauthorizedAccessException("User is not authorized to create tasks in this project");
        }

        var assigneeId = string.IsNullOrWhiteSpace(request.AssigneeId) ? null : request.AssigneeId.Trim();
        if (!string.IsNullOrEmpty(assigneeId) && !IsProjectMember(project, assigneeId))
        {
            throw new InvalidOperationException("Assignee must be a member of the project");
        }

        var now = DateTime.UtcNow;
        var task = new DomainTask
        {
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            ProjectId = projectId,
            AssigneeId = assigneeId,
            Priority = request.Priority,
            Status = DomainTaskStatus.ToDo,
            CreatedBy = userId,
            DueDate = request.DueDate,
            CreatedTimestamp = now,
            UpdatedTimestamp = now
        };

        var createdTask = await _taskRepository.CreateAsync(task, ct);

        var historyEntry = new TaskHistory
        {
            TaskId = createdTask.Id,
            ChangedBy = userId,
            ChangeType = TaskHistoryChangeType.StatusChanged,
            OldValue = null,
            NewValue = createdTask.Status.ToString(),
            ChangedTimestamp = now
        };

        _context.TaskHistory.Add(historyEntry);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Task {TaskId} created in project {ProjectId} by user {UserId}", createdTask.Id, projectId, userId);
        _logger.LogDebug("Task {TaskId} create event queued for realtime broadcast", createdTask.Id);

        return new CreateTaskResponse
        {
            Id = createdTask.Id,
            ProjectId = createdTask.ProjectId,
            Title = createdTask.Title,
            Description = createdTask.Description,
            Status = createdTask.Status,
            Priority = createdTask.Priority,
            AssigneeId = createdTask.AssigneeId,
            CreatedBy = createdTask.CreatedBy,
            DueDate = createdTask.DueDate,
            CreatedTimestamp = createdTask.CreatedTimestamp,
            UpdatedTimestamp = createdTask.UpdatedTimestamp
        };
    }

    private static bool IsProjectMember(Project project, string userId)
    {
        if (project.OwnerId == userId)
        {
            return true;
        }

        return project.Members.Any(m => m.UserId == userId);
    }
}
