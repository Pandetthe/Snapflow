using FluentValidation;
using Snapflow.Domain.Tags;

namespace Snapflow.Application.Tags.Update;

internal sealed class UpdateTagValidator : AbstractValidator<UpdateTagCommand>
{
    public UpdateTagValidator()
    {
        RuleFor(t => t.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(TagOptions.MaxTitleLength)
            .WithMessage($"Title must not exceed {TagOptions.MaxTitleLength} characters.")
            .MinimumLength(TagOptions.MinTitleLength)
            .WithMessage($"Title must be at least {TagOptions.MinTitleLength} characters long.");

        RuleFor(t => t.Color)
            .IsInEnum().WithMessage("Color is not a known tag color.");
    }
}
