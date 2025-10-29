using FluentValidation;
using Snapflow.Domain.Boards;

namespace Snapflow.Application.Boards.Create;

internal sealed class CreateBoardCommandValidator : AbstractValidator<CreateBoardCommand>
{
    public CreateBoardCommandValidator()
    {
        RuleFor(b => b.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(BoardOptions.MaxTitleLength).WithMessage($"Title must not exceed {BoardOptions.MaxTitleLength} characters.")
            .MinimumLength(BoardOptions.MinTitleLength).WithMessage($"Title must be at least {BoardOptions.MinTitleLength} characters long.");

        RuleFor(b => b.Description)
            .MaximumLength(BoardOptions.MaxDescriptionLength).WithMessage($"Description must not exceed {BoardOptions.MaxDescriptionLength} characters.");
    }
}

