using FluentValidation;
using Snapflow.Domain.Swimlanes;

namespace Snapflow.Application.Swimlanes.Update;

internal sealed class UpdateSwimlaneCommandValidator : AbstractValidator<UpdateSwimlaneCommand>
{
    public UpdateSwimlaneCommandValidator()
    {
        RuleFor(b => b.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(SwimlaneOptions.MaxTitleLength)
            .WithMessage($"Title must not exceed {SwimlaneOptions.MaxTitleLength} characters.")
            .MinimumLength(SwimlaneOptions.MinTitleLength)
            .WithMessage($"Title must be at least {SwimlaneOptions.MinTitleLength} characters long.");
    }
}