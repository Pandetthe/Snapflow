using FluentValidation;
using Snapflow.Domain.Swimlanes;

namespace Snapflow.Application.Swimlanes.Update;

internal sealed class UpdateSwimlaneValidator : AbstractValidator<UpdateSwimlaneCommand>
{
    public UpdateSwimlaneValidator()
    {
        RuleFor(s => s.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(SwimlaneOptions.MaxTitleLength)
            .WithMessage($"Title must not exceed {SwimlaneOptions.MaxTitleLength} characters.")
            .MinimumLength(SwimlaneOptions.MinTitleLength)
            .WithMessage($"Title must be at least {SwimlaneOptions.MinTitleLength} characters long.");

        RuleFor(s => s.Height)
            .GreaterThanOrEqualTo(SwimlaneOptions.MinHeight)
            .WithMessage($"Height must be greater than or equal to {SwimlaneOptions.MinHeight}.");
    }
}