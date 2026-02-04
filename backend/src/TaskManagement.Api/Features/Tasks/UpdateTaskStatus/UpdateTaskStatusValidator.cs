using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;
using TaskManagement.Api.Data.Repositories;
using TaskManagement.Api.Domain.Tasks;
using DomainTaskStatus = TaskManagement.Api.Domain.Tasks.TaskStatus;

namespace TaskManagement.Api.Features.Tasks.UpdateTaskStatus;

public class UpdateTaskStatusValidator : AbstractValidator<UpdateTaskStatusRequest>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly TaskManagementDbContext _context;
    private int _taskId;
    private string _userId = string.Empty;

    public UpdateTaskStatusValidator(
        ITaskRepository taskRepository,
        IProjectRepository projectRepository,
        TaskManagementDbContext context)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _context = context;

        RuleFor(x => x.NewStatus)
            .IsInEnum()
            .WithMessage("Status must be a valid TaskStatus value (ToDo, InProgress, InReview, Done)");
    }

    public void SetContext(int taskId, string userId)
    {
        _taskId = taskId;
        _userId = userId;
    }

    public async System.Threading.Tasks.Task<FluentValidation.Results.ValidationResult> ValidateWithContextAsync(UpdateTaskStatusRequest request)
    {
        // Validate basic request
        var basicValidation = await this.ValidateAsync(request);
        if (!basicValidation.IsValid)
            return basicValidation;

        // Additional authorization check
        var task = await _taskRepository.GetByIdAsync(_taskId);
        if (task == null)
        {
            basicValidation.Errors.Add(new FluentValidation.Results.ValidationFailure(
                nameof(UpdateTaskStatusRequest.NewStatus),
                "Task not found"));
            return basicValidation;
        }

        var project = await _projectRepository.GetByIdAsync(task.ProjectId);
        if (project == null)
        {
            basicValidation.Errors.Add(new FluentValidation.Results.ValidationFailure(
                nameof(UpdateTaskStatusRequest.NewStatus),
                "Project not found"));
            return basicValidation;
        }

        // Check if user is a project member
        var isMember = await _context.ProjectMembers
            .AsNoTracking()
            .AnyAsync(pm => pm.ProjectId == project.Id && pm.UserId == _userId);

        if (!isMember)
        {
            basicValidation.Errors.Add(new FluentValidation.Results.ValidationFailure(
                nameof(UpdateTaskStatusRequest.NewStatus),
                "User is not a member of this project"));
        }

        return basicValidation;
    }
}
