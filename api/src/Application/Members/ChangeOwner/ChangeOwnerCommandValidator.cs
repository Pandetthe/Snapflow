using FluentValidation;

namespace Snapflow.Application.Members.ChangeOwner;

internal sealed class ChangeOwnerCommandValidator : AbstractValidator<ChangeOwnerCommand>
{
    public ChangeOwnerCommandValidator()
    {
        RuleFor(c => c.UserId)
            .GreaterThan(0).WithMessage("Invalid user identifier.");
        RuleFor(c => c.BoardId)
            .GreaterThan(0).WithMessage("Invalid board identifier.");
    }
}