using FluentValidation;
using Snapflow.Domain.Cards;

namespace Snapflow.Application.Cards.Update;

internal sealed class CreateCardCommandValidator : AbstractValidator<UpdateCardCommand>
{
    public CreateCardCommandValidator()
    {
        RuleFor(c => c.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(CardOptions.MaxTitleLength)
            .WithMessage($"Title must not exceed {CardOptions.MaxTitleLength} characters.")
            .MinimumLength(CardOptions.MinTitleLength)
            .WithMessage($"Title must be at least {CardOptions.MinTitleLength} characters long.");

        RuleFor(c => c.Description)
            .MaximumLength(CardOptions.MaxDescriptionLength)
            .WithMessage($"Description must not exceed {CardOptions.MaxDescriptionLength} characters.");
    }
}