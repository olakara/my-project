using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Data;

namespace TaskManagement.Api.Features.Tasks.AssignTask;

public class AssignTaskValidator : AbstractValidator<AssignTaskRequest>
{
    private readonly TaskManagementDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AssignTaskValidator(TaskManagementDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;

        RuleFor(x => x.AssigneeId)
            .MustAsync(AssigneeIsProjectMemberAsync)
            .When(x => !string.IsNullOrWhiteSpace(x.AssigneeId))
            .WithMessage("Assignee must be a member of the project");
    }

    private async System.Threading.Tasks.Task<bool> AssigneeIsProjectMemberAsync(
        AssignTaskRequest request,
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

        // Get taskId from route
        if (!httpContext.GetRouteData().Values.TryGetValue("taskId", out var taskIdObj) || 
            !int.TryParse(taskIdObj?.ToString(), out var taskId))
        {
            return false;
        }

        // Get the task to find its project
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == taskId, ct);
        if (task == null)
        {
            return false;
        }

        // Verify assignee is a member of the task's project
        var isMember = await _context.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == task.ProjectId && pm.UserId == assigneeId.Trim(), ct);

        return isMember;
    }
}
