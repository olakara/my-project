using System.Security.Claims;
using FluentValidation;
using TaskManagement.Api.Data.Repositories;
using TaskManagement.Api.Domain.Projects;

namespace TaskManagement.Api.Features.Projects.UpdateProject;

public class UpdateProjectValidator : AbstractValidator<UpdateProjectRequest>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UpdateProjectValidator(IProjectRepository projectRepository, IHttpContextAccessor httpContextAccessor)
    {
        _projectRepository = projectRepository;
        _httpContextAccessor = httpContextAccessor;

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Project name cannot exceed 100 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Project description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x)
            .MustAsync(UserCanManageProjectAsync)
            .WithMessage("User is not authorized to update this project");
    }

    private async System.Threading.Tasks.Task<bool> UserCanManageProjectAsync(UpdateProjectRequest request, CancellationToken ct)
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
