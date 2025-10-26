using FluentValidation;
using Snapflow.Domain.Boards;

namespace Snapflow.Application.Boards.Create;

internal sealed class CreateBoardCommandValidator : AbstractValidator<CreateBoardCommand>
{
    public CreateBoardCommandValidator()
    {
        RuleFor(c => c.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(BoardOptions.MaxTitleLength).WithMessage("Title must not exceed 100 characters.")
            .MinimumLength(BoardOptions.MinTitleLength).WithMessage("Title must be at least 3 characters long.");

        RuleFor(c => c.Description)
            .MaximumLength(BoardOptions.MaxDescriptionLength).WithMessage("Description must not exceed 500 characters.");
    }
}

