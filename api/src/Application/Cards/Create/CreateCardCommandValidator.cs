using FluentValidation;
using Snapflow.Domain.Cards;

namespace Snapflow.Application.Cards.Create;

internal sealed class CreateCardCommandValidator : AbstractValidator<CreateCardCommand>
{
    public CreateCardCommandValidator()
    {
        RuleFor(c => c.BoardId)
            .GreaterThan(0).WithMessage("Invalid board identifier.");
        RuleFor(c => c.SwimlaneId)
            .GreaterThan(0).WithMessage("Invalid swimlane identifier.");
        RuleFor(c => c.ListId)
            .GreaterThan(0).WithMessage("Invalid list identifier.");
        RuleFor(b => b.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(CardOptions.MaxTitleLength).WithMessage($"Title must not exceed {CardOptions.MaxTitleLength} characters.")
            .MinimumLength(CardOptions.MinTitleLength).WithMessage($"Title must be at least {CardOptions.MinTitleLength} characters long.");
        RuleFor(b => b.Description)
            .MaximumLength(CardOptions.MaxDescriptionLength).WithMessage($"Description must not exceed {CardOptions.MaxDescriptionLength} characters.");
    }
}