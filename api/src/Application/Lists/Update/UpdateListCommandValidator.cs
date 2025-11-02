using FluentValidation;
using Snapflow.Domain.Lists;

namespace Snapflow.Application.Lists.Update;

internal sealed class UpdateListCommandValidator : AbstractValidator<UpdateListCommand>
{
    public UpdateListCommandValidator()
    {
        RuleFor(b => b.Title)
             .NotEmpty().WithMessage("Title is required.")
             .MaximumLength(ListOptions.MaxTitleLength)
             .WithMessage($"Title must not exceed {ListOptions.MaxTitleLength} characters.")
             .MinimumLength(ListOptions.MinTitleLength)
             .WithMessage($"Title must be at least {ListOptions.MinTitleLength} characters long.");
    }
}