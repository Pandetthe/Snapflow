using FluentValidation;
using Snapflow.Domain.Members;

namespace Snapflow.Application.Members.Add;

internal sealed class AddMemberCommandValidator : AbstractValidator<AddMemberCommand>
{
    public AddMemberCommandValidator()
    {
        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Role must be a valid role.")
            .NotEqual(MemberRole.Owner).WithMessage("Role cannot be set to Owner.");
    }
}