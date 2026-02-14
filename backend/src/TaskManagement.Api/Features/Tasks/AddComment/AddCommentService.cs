using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.Data.Repositories;
using TaskManagement.Api.Domain.Tasks;
using TaskManagement.Api.Hubs;

namespace TaskManagement.Api.Features.Tasks.AddComment;

public interface IAddCommentService
{
    System.Threading.Tasks.Task<AddCommentResponse> AddCommentAsync(
        int taskId,
        string userId,
        AddCommentRequest request,
        CancellationToken ct = default);
}

public class AddCommentService : IAddCommentService
{
    private readonly TaskManagementDbContext _context;
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IHubContext<TaskManagementHub> _hubContext;
    private readonly ILogger<AddCommentService> _logger;

    public AddCommentService(
        TaskManagementDbContext context,
        ITaskRepository taskRepository,
        IProjectRepository projectRepository,
        IHubContext<TaskManagementHub> hubContext,
        ILogger<AddCommentService> logger)
    {
        _context = context;
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<AddCommentResponse> AddCommentAsync(
        int taskId,
        string userId,
        AddCommentRequest request,
        CancellationToken ct = default)
    {
        // Fetch task with project information
        var task = await _taskRepository.GetByIdAsync(taskId, ct);
        if (task == null)
        {
            throw new KeyNotFoundException("Task not found");
        }

        // Verify user is a member of the project
        var isMember = await _context.ProjectMembers
            .AsNoTracking()
            .AnyAsync(pm => pm.ProjectId == task.ProjectId && pm.UserId == userId, ct);

        if (!isMember)
        {
            throw new UnauthorizedAccessException("User is not a member of this project");
        }

        // Fetch author details for response
        var author = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (author == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        // Create comment
        var now = DateTime.UtcNow;
        var comment = new Comment
        {
            TaskId = taskId,
            AuthorId = userId,
            Content = request.Content.Trim(),
            CreatedTimestamp = now,
            EditedTimestamp = null
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Comment {CommentId} added to task {TaskId} by user {UserId}",
            comment.Id, taskId, userId);

        var response = new AddCommentResponse
        {
            Id = comment.Id,
            TaskId = comment.TaskId,
            Content = comment.Content,
            AuthorId = comment.AuthorId,
            AuthorName = $"{author.FirstName} {author.LastName}".Trim(),
            CreatedTimestamp = comment.CreatedTimestamp,
            EditedTimestamp = comment.EditedTimestamp
        };

        // Broadcast comment to all project members via SignalR
        await _hubContext.Clients.Group($"project-{task.ProjectId}").SendAsync("CommentAdded", new
        {
            id = comment.Id,
            taskId = comment.TaskId,
            projectId = task.ProjectId,
            content = comment.Content,
            authorId = comment.AuthorId,
            authorName = response.AuthorName,
            createdTimestamp = comment.CreatedTimestamp
        }, ct);

        _logger.LogDebug(
            "Comment {CommentId} broadcasted to project {ProjectId} members",
            comment.Id, task.ProjectId);

        return response;
    }
}
