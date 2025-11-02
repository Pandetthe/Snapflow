using FluentValidation;
using Snapflow.Application.Cards.Update;
using Snapflow.Domain.Cards;

namespace Snapflow.Application.Cards.Update;

internal sealed class CreateCardCommandValidator : AbstractValidator<UpdateCardCommand>
{
    public CreateCardCommandValidator()
    {
        RuleFor(b => b.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(CardOptions.MaxTitleLength)
            .WithMessage($"Title must not exceed {CardOptions.MaxTitleLength} characters.")
            .MinimumLength(CardOptions.MinTitleLength)
            .WithMessage($"Title must be at least {CardOptions.MinTitleLength} characters long.");
        RuleFor(b => b.Description)
            .MaximumLength(CardOptions.MaxDescriptionLength)
            .WithMessage($"Description must not exceed {CardOptions.MaxDescriptionLength} characters.");
    }
}