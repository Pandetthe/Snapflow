using FluentValidation;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.Logins.Remove;

internal sealed class RemoveLoginValidator : AbstractValidator<RemoveLoginCommand>
{
    public RemoveLoginValidator()
    {
        RuleFor(c => c.Provider)
            .NotEmpty()
                .WithMessage("Provider is required.")
            .MaximumLength(UserOptions.MaxLoginProviderLength)
                .WithMessage($"Provider must not exceed {UserOptions.MaxLoginProviderLength} characters.");
    }
}
