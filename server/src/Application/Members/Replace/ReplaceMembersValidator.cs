using FluentValidation;
using Snapflow.Domain.Members;

namespace Snapflow.Application.Members.Replace;

internal sealed class ReplaceMembersValidator : AbstractValidator<ReplaceMembersCommand>
{
    public ReplaceMembersValidator()
    {
        RuleFor(x => x.Members)
            .NotNull().WithMessage("Members are required.");

        RuleForEach(x => x.Members).ChildRules(member =>
        {
            member.RuleFor(m => m.Role)
                .IsInEnum().WithMessage("Role must be a valid role.")
                .NotEqual(MemberRole.Owner).WithMessage("Role cannot be set to Owner.");
        });
    }
}