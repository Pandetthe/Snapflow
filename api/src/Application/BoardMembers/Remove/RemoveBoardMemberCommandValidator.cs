using FluentValidation;

namespace Snapflow.Application.BoardMembers.Remove;

internal sealed class RemoveBoardMemberCommandValidator : AbstractValidator<RemoveBoardMemberCommand>
{
    public RemoveBoardMemberCommandValidator()
    {
        RuleFor(c => c.UserId)
            .GreaterThan(0).WithMessage("Invalid user identifier.");
        RuleFor(c => c.BoardId)
            .GreaterThan(0).WithMessage("Invalid board identifier.");
    }
}