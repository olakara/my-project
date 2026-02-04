using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;

namespace TaskManagement.Api.Features.Tasks.CreateTask;

public class CreateTaskValidator : AbstractValidator<CreateTaskRequest>
{
    private readonly TaskManagementDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateTaskValidator(TaskManagementDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required")
            .MinimumLength(1).WithMessage("Task title must be at least 1 character")
            .MaximumLength(200).WithMessage("Task title cannot exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(5000).WithMessage("Task description cannot exceed 5000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.DueDate)
            .Must(date => date == null || date.Value > DateTime.UtcNow)
            .WithMessage("Due date must be in the future");

        RuleFor(x => x.AssigneeId)
            .MustAsync(AssigneeIsProjectMemberAsync)
            .When(x => !string.IsNullOrWhiteSpace(x.AssigneeId))
            .WithMessage("Assignee must be a member of the project");
    }

    private async System.Threading.Tasks.Task<bool> AssigneeIsProjectMemberAsync(
        CreateTaskRequest request,
        string? assigneeId,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(assigneeId))
        {
            return true;
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return false;
        }

        if (!int.TryParse(httpContext.Request.RouteValues["projectId"]?.ToString(), out var projectId))
        {
            return false;
        }

        return await _context.ProjectMembers
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == assigneeId, ct);
    }
}
