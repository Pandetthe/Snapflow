using FluentValidation;
using Snapflow.Domain.Lists;

namespace Snapflow.Application.Lists.Create;

internal sealed class CreateListValidator : AbstractValidator<CreateListCommand>
{
    public CreateListValidator()
    {
        RuleFor(l => l.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(ListOptions.MaxTitleLength)
            .WithMessage($"Title must not exceed {ListOptions.MaxTitleLength} characters.")
            .MinimumLength(ListOptions.MinTitleLength)
            .WithMessage($"Title must be at least {ListOptions.MinTitleLength} characters long.");

        RuleFor(l => l.Width)
            .GreaterThanOrEqualTo(ListOptions.MinWidth)
            .WithMessage($"Width must be greater than  or equal to{ListOptions.MinWidth}.");
    }
}