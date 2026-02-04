using System.Security.Claims;
using FluentValidation;
using TaskManagement.Api.Data.Repositories;
using TaskManagement.Api.Domain.Projects;

namespace TaskManagement.Api.Features.Tasks.UpdateTask;

public class UpdateTaskValidator : AbstractValidator<UpdateTaskRequest>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UpdateTaskValidator(
        ITaskRepository taskRepository,
        IProjectRepository projectRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _httpContextAccessor = httpContextAccessor;

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title cannot be empty")
            .MaximumLength(200).WithMessage("Task title cannot exceed 200 characters")
            .When(x => x.Title != null);

        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("Task description cannot exceed 5000 characters")
            .When(x => x.Description != null);

        RuleFor(x => x.Priority)
            .IsInEnum().WithMessage("Task priority is invalid")
            .When(x => x.Priority.HasValue);

        RuleFor(x => x.DueDate)
            .Must(date => date == null || date.Value > DateTime.UtcNow)
            .WithMessage("Due date must be in the future");

        RuleFor(x => x)
            .MustAsync(UserCanEditTaskAsync)
            .WithMessage("User is not authorized to update this task");
    }

    private async System.Threading.Tasks.Task<bool> UserCanEditTaskAsync(UpdateTaskRequest request, CancellationToken ct)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return false;
        }

        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return false;
        }

        if (!int.TryParse(httpContext.Request.RouteValues["taskId"]?.ToString(), out var taskId))
        {
            return false;
        }

        var task = await _taskRepository.GetByIdAsync(taskId, ct);
        if (task == null)
        {
            return false;
        }

        if (task.CreatedBy == userId)
        {
            return true;
        }

        var project = await _projectRepository.GetByIdAsync(task.ProjectId, ct);
        if (project == null)
        {
            return false;
        }

        if (project.OwnerId == userId)
        {
            return true;
        }

        var role = project.GetUserRole(userId);
        return role == ProjectRole.Manager;
    }
}
