using FluentValidation;
using Snapflow.Domain.Lists;

namespace Snapflow.Application.Lists.Delete;

internal sealed class DeleteListCommandValidator : AbstractValidator<DeleteListCommand>
{
    public DeleteListCommandValidator()
    {
        RuleFor(c => c.Id)
            .GreaterThan(0).WithMessage("Invalid list identifier.");
        RuleFor(c => c.BoardId)
            .GreaterThan(0).WithMessage("Invalid board identifier.");
        RuleFor(c => c.SwimlaneId)
            .GreaterThan(0).WithMessage("Invalid swimlane identifier.");
    }
}