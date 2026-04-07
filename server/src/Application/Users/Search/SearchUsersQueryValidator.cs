using FluentValidation;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Search;

internal sealed class SearchUsersQueryValidator : AbstractValidator<SearchUsersQuery>
{
    public SearchUsersQueryValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(UserOptions.MaxUserNameLength)
            .WithMessage($"Name must not exceed {UserOptions.MaxUserNameLength} characters.");
        RuleFor(x => x.ExcludedIds)
            .ForEach(id => id.GreaterThan(0).WithMessage("Excluded user IDs must be greater than 0."));
    }
}
