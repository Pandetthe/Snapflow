using FluentValidation;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Members;

namespace Snapflow.Application.Boards.Create;

internal sealed class CreateBoardValidator : AbstractValidator<CreateBoardCommand>
{
    public CreateBoardValidator()
    {
        RuleFor(b => b.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(BoardOptions.MaxTitleLength)
            .WithMessage($"Title must not exceed {BoardOptions.MaxTitleLength} characters.")
            .MinimumLength(BoardOptions.MinTitleLength)
            .WithMessage($"Title must be at least {BoardOptions.MinTitleLength} characters long.");

        RuleFor(b => b.Description)
            .MaximumLength(BoardOptions.MaxDescriptionLength)
            .WithMessage($"Description must not exceed {BoardOptions.MaxDescriptionLength} characters.");

        RuleForEach(b => b.Members)
            .ChildRules(member =>
            {
                member.RuleFor(m => m.UserId)
                    .GreaterThan(0).WithMessage("User ID must be greater than 0.");

                member.RuleFor(m => m.Role)
                    .IsInEnum().WithMessage("Role must be a valid role.")
                    .NotEqual(MemberRole.Owner).WithMessage("Role cannot be set to Owner.");
            });
    }
}

