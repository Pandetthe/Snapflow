using FluentValidation;

namespace Snapflow.Application.Cards.Move;

internal sealed class MoveCardValidator : AbstractValidator<MoveCardCommand>
{
    public MoveCardValidator()
    {
        RuleFor(c => c.BeforeId)
            .NotEqual(c => c.Id).WithMessage("Card cannot be placed before itself.")
            .When(c => c.BeforeId.HasValue);
    }
}
