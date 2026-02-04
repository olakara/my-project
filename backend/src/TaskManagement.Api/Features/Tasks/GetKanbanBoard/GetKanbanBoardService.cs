using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.Data.Repositories;
using TaskManagement.Api.Domain.Projects;
using TaskManagement.Api.Domain.Tasks;
using DomainTask = TaskManagement.Api.Domain.Tasks.Task;
using DomainTaskStatus = TaskManagement.Api.Domain.Tasks.TaskStatus;

namespace TaskManagement.Api.Features.Tasks.GetKanbanBoard;

public record TaskCardDto(
    int Id,
    string Title,
    string? Description,
    TaskPriority Priority,
    string? AssigneeName,
    DateTime? DueDate,
    DateTime CreatedTimestamp,
    bool IsOverdue);

public record KanbanBoardDto(
    List<KanbanColumnDto> Columns,
    int TotalTasks,
    int CurrentPage,
    int PageSize,
    int TotalPages);

public record KanbanColumnDto(
    DomainTaskStatus Status,
    string StatusLabel,
    List<TaskCardDto> Tasks,
    int Count);

public interface IGetKanbanBoardService
{
    System.Threading.Tasks.Task<KanbanBoardDto> GetKanbanBoardAsync(
        int projectId,
        string userId,
        int page = 1,
        int pageSize = 50,
        string? assigneeFilter = null,
        TaskPriority? priorityFilter = null,
        DateTime? dueDateFilter = null,
        CancellationToken ct = default);
}

public class GetKanbanBoardService : IGetKanbanBoardService
{
    private readonly TaskManagementDbContext _context;
    private readonly IProjectRepository _projectRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly ILogger<GetKanbanBoardService> _logger;

    public GetKanbanBoardService(
        TaskManagementDbContext context,
        IProjectRepository projectRepository,
        ITaskRepository taskRepository,
        ILogger<GetKanbanBoardService> logger)
    {
        _context = context;
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<KanbanBoardDto> GetKanbanBoardAsync(
        int projectId,
        string userId,
        int page = 1,
        int pageSize = 50,
        string? assigneeFilter = null,
        TaskPriority? priorityFilter = null,
        DateTime? dueDateFilter = null,
        CancellationToken ct = default)
    {
        // Verify project exists and user is a member
        var project = await _projectRepository.GetByIdAsync(projectId, ct);
        if (project == null)
        {
            throw new KeyNotFoundException("Project not found");
        }

        var isMember = await _context.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId, ct);

        if (!isMember)
        {
            throw new UnauthorizedAccessException("User is not a member of this project");
        }

        // Build base query with filters
        var query = _context.Tasks
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId)
            .Include(t => t.Assignee)
            .AsQueryable();

        // Apply optional filters
        if (!string.IsNullOrEmpty(assigneeFilter))
        {
            query = query.Where(t => t.AssigneeId == assigneeFilter);
        }

        if (priorityFilter.HasValue)
        {
            query = query.Where(t => t.Priority == priorityFilter.Value);
        }

        if (dueDateFilter.HasValue)
        {
            var targetDate = dueDateFilter.Value.Date;
            query = query.Where(t => t.DueDate.HasValue && 
                                     t.DueDate.Value.Date <= targetDate);
        }

        var totalTasks = await query.CountAsync(ct);
        var totalPages = (totalTasks + pageSize - 1) / pageSize;

        // Ensure page is valid
        if (page < 1)
            page = 1;
        if (page > totalPages && totalPages > 0)
            page = totalPages;

        // Get tasks grouped by status with pagination
        var tasks = await query
            .OrderByDescending(t => t.CreatedTimestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var columns = BuildColumns(tasks);

        _logger.LogInformation(
            "Kanban board retrieved for project {ProjectId} by user {UserId}. Total tasks: {TotalTasks}, Page: {Page}/{TotalPages}",
            projectId, userId, totalTasks, page, totalPages);

        return new KanbanBoardDto(
            Columns: columns,
            TotalTasks: totalTasks,
            CurrentPage: page,
            PageSize: pageSize,
            TotalPages: totalPages);
    }

    private List<KanbanColumnDto> BuildColumns(List<DomainTask> tasks)
    {
        var statuses = Enum.GetValues(typeof(DomainTaskStatus)).Cast<DomainTaskStatus>().ToList();
        var columns = new List<KanbanColumnDto>();

        foreach (var status in statuses)
        {
            var statusTasks = tasks
                .Where(t => t.Status == status)
                .Select(t => new TaskCardDto(
                    Id: t.Id,
                    Title: t.Title,
                    Description: t.Description,
                    Priority: t.Priority,
                    AssigneeName: t.Assignee?.FirstName + " " + t.Assignee?.LastName,
                    DueDate: t.DueDate,
                    CreatedTimestamp: t.CreatedTimestamp,
                    IsOverdue: t.IsOverdue))
                .ToList();

            columns.Add(new KanbanColumnDto(
                Status: status,
                StatusLabel: status.ToString(),
                Tasks: statusTasks,
                Count: statusTasks.Count));
        }

        return columns;
    }
}
