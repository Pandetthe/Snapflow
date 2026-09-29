using FluentValidation;
using Snapflow.Domain.Members;

namespace Snapflow.Application.Members.ChangeRole;

internal sealed class ChangeMemberRoleValidator : AbstractValidator<ChangeMemberRoleCommand>
{
    public ChangeMemberRoleValidator()
    {
        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Role must be a valid role.")
            .NotEqual(MemberRole.Owner).WithMessage("Role cannot be set to Owner.");
    }
}