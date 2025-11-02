using FluentValidation;
using Snapflow.Domain.Swimlanes;

namespace Snapflow.Application.Swimlanes.Create;

internal sealed class CreateSwimlaneCommandValidator : AbstractValidator<CreateSwimlaneCommand>
{
    public CreateSwimlaneCommandValidator()
    {
        RuleFor(b => b.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(SwimlaneOptions.MaxTitleLength)
            .WithMessage($"Title must not exceed {SwimlaneOptions.MaxTitleLength} characters.")
            .MinimumLength(SwimlaneOptions.MinTitleLength)
            .WithMessage($"Title must be at least {SwimlaneOptions.MinTitleLength} characters long.");
    }
}