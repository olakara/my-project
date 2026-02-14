using FluentValidation;

namespace TaskManagement.Api.Features.Tasks.AddComment;

public class AddCommentValidator : AbstractValidator<AddCommentRequest>
{
    public AddCommentValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Comment content is required")
            .MinimumLength(1)
            .WithMessage("Comment content must be at least 1 character")
            .MaximumLength(5000)
            .WithMessage("Comment content cannot exceed 5000 characters");
    }
}
