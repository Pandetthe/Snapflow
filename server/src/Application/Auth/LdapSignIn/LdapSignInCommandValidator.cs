using FluentValidation;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Auth.LdapSignIn;

internal sealed class LdapSignInCommandValidator : AbstractValidator<LdapSignInCommand>
{
    public LdapSignInCommandValidator()
    {
        RuleFor(c => c.UserName)
            .NotEmpty()
            .WithMessage("User name is required.")
            .MaximumLength(UserOptions.MaxEmailLength)
            .WithMessage($"User name must not exceed {UserOptions.MaxEmailLength} characters.");

        RuleFor(c => c.Password)
            .NotEmpty()
            .WithMessage("Password is required.");
    }
}
