using FluentValidation;

namespace Snapflow.Application.BoardMembers.ChangeOwner;

internal sealed class ChangeBoardOwnerCommandValidator : AbstractValidator<ChangeBoardOwnerCommand>
{
    public ChangeBoardOwnerCommandValidator()
    {
        RuleFor(c => c.UserId)
            .GreaterThan(0).WithMessage("Invalid user identifier.");
        RuleFor(c => c.BoardId)
            .GreaterThan(0).WithMessage("Invalid board identifier.");
    }
}