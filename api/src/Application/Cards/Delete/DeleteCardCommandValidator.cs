using FluentValidation;

namespace Snapflow.Application.Cards.Delete;

internal sealed class DeleteCardCommandValidator : AbstractValidator<DeleteCardCommand>
{
    public DeleteCardCommandValidator()
    {
        RuleFor(c => c.SwimlaneId)
    .GreaterThan(0).WithMessage("Invalid swimlane identifier.");
        RuleFor(c => c.SwimlaneId)
    .GreaterThan(0).WithMessage("Invalid swimlane identifier.");
        RuleFor(c => c.SwimlaneId)
    .GreaterThan(0).WithMessage("Invalid swimlane identifier.");
        RuleFor(c => c.SwimlaneId)
    .GreaterThan(0).WithMessage("Invalid swimlane identifier.");
    }
}