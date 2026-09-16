using FluentValidation;

namespace Snapflow.Application.Boards.ChangeVisibility;

internal sealed class ChangeBoardVisibilityValidator : AbstractValidator<ChangeBoardVisibilityCommand>
{
    public ChangeBoardVisibilityValidator()
    {
        RuleFor(b => b.Visibility)
            .IsInEnum().WithMessage("Visibility is not valid.");
    }
}
