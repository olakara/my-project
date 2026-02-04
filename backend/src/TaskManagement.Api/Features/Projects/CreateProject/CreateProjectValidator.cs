using FluentValidation;

namespace TaskManagement.Api.Features.Projects.CreateProject;

public class CreateProjectValidator : AbstractValidator<CreateProjectRequest>
{
    public CreateProjectValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required")
            .MinimumLength(1).WithMessage("Project name must be at least 1 character")
            .MaximumLength(100).WithMessage("Project name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Project description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}
