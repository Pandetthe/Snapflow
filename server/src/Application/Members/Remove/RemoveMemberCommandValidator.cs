using FluentValidation;

namespace Snapflow.Application.Members.Remove;

internal sealed class RemoveMemberCommandValidator : AbstractValidator<RemoveMemberCommand>
{
    public RemoveMemberCommandValidator()
    {
        RuleFor(c => c.UserId)
            .GreaterThan(0).WithMessage("Invalid user identifier.");
        RuleFor(c => c.BoardId)
            .GreaterThan(0).WithMessage("Invalid board identifier.");
    }
}