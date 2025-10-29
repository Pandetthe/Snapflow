using FluentValidation;
using Snapflow.Domain.BoardMembers;

namespace Snapflow.Application.BoardMembers.Add;

internal sealed class AddBoardMemberCommandValidator : AbstractValidator<AddBoardMemberCommand>
{
    public AddBoardMemberCommandValidator()
    {
        RuleFor(c => c.UserId)
            .GreaterThan(0).WithMessage("Invalid user identifier.");
        RuleFor(c => c.BoardId)
            .GreaterThan(0).WithMessage("Invalid board identifier.");
        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Role must be a valid BoardMemberRole.")
            .NotEqual(BoardMemberRole.Owner).WithMessage("Role cannot be set to Owner.");
    }
}