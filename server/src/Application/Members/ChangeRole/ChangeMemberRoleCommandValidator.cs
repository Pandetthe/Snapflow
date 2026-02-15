using FluentValidation;
using Snapflow.Domain.Members;

namespace Snapflow.Application.Members.ChangeRole;

internal sealed class ChangeMemberRoleCommandValidator : AbstractValidator<ChangeMemberRoleCommand>
{
    public ChangeMemberRoleCommandValidator()
    {
        RuleFor(c => c.UserId)
            .GreaterThan(0).WithMessage("Invalid user identifier.");
        RuleFor(c => c.BoardId)
            .GreaterThan(0).WithMessage("Invalid board identifier.");
        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Role must be a valid BoardMemberRole.")
            .NotEqual(MemberRole.Owner).WithMessage("Role cannot be set to Owner.");
    }
}