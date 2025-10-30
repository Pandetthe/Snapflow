using FluentValidation;

namespace Snapflow.Application.Swimlanes.Delete;

internal sealed class DeleteSwimlaneCommandValidator : AbstractValidator<DeleteSwimlaneCommand>
{
    public DeleteSwimlaneCommandValidator()
    {
        RuleFor(c => c.Id)
            .GreaterThan(0).WithMessage("Invalid swimlane identifier.");
        RuleFor(c => c.BoardId)
            .GreaterThan(0).WithMessage("Invalid board identifier.");
    }
}