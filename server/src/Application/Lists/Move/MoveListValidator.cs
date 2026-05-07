using FluentValidation;

namespace Snapflow.Application.Lists.Move;

internal sealed class MoveListValidator : AbstractValidator<MoveListCommand>
{
    public MoveListValidator()
    {
        RuleFor(l => l.BeforeId)
            .NotEqual(l => l.Id).WithMessage("List cannot be placed before itself.")
            .When(l => l.BeforeId.HasValue);
    }
}
