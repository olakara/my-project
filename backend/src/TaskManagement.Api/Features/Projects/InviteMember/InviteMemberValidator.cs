using System.Security.Claims;
using FluentValidation;
using TaskManagement.Api.Data.Repositories;
using TaskManagement.Api.Domain.Projects;

namespace TaskManagement.Api.Features.Projects.InviteMember;

public class InviteMemberValidator : AbstractValidator<InviteMemberRequest>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public InviteMemberValidator(IProjectRepository projectRepository, IHttpContextAccessor httpContextAccessor)
    {
        _projectRepository = projectRepository;
        _httpContextAccessor = httpContextAccessor;

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters");

        RuleFor(x => x.Role)
            .Must(role => role == ProjectRole.Manager || role == ProjectRole.Member)
            .WithMessage("Role must be Manager or Member");

        RuleFor(x => x)
            .MustAsync(UserCanInviteAsync)
            .WithMessage("User is not authorized to invite members to this project");
    }

    private async System.Threading.Tasks.Task<bool> UserCanInviteAsync(InviteMemberRequest request, CancellationToken ct)
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

        if (!int.TryParse(httpContext.Request.RouteValues["projectId"]?.ToString(), out var projectId))
        {
            return false;
        }

        var project = await _projectRepository.GetByIdAsync(projectId, ct);
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
