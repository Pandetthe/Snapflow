using FluentValidation;

namespace Snapflow.Application.Swimlanes.Move;

internal sealed class MoveSwimlaneValidator : AbstractValidator<MoveSwimlaneCommand>
{
    public MoveSwimlaneValidator()
    {
        RuleFor(s => s.BeforeId)
            .NotEqual(s => s.Id).WithMessage("Swimlane cannot be placed before itself.")
            .When(s => s.BeforeId.HasValue);
    }
}
