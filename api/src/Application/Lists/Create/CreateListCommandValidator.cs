using FluentValidation;
using Snapflow.Domain.Lists;

namespace Snapflow.Application.Lists.Create;

internal sealed class CreateListCommandValidator : AbstractValidator<CreateListCommand>
{
    public CreateListCommandValidator()
    {
        RuleFor(c => c.BoardId)
            .GreaterThan(0).WithMessage("Invalid board identifier.");
        RuleFor(c => c.SwimlaneId)
            .GreaterThan(0).WithMessage("Invalid swimlane identifier.");
        RuleFor(b => b.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(ListOptions.MaxTitleLength).WithMessage($"Title must not exceed {ListOptions.MaxTitleLength} characters.")
            .MinimumLength(ListOptions.MinTitleLength).WithMessage($"Title must be at least {ListOptions.MinTitleLength} characters long.");
    }
}