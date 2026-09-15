using FluentValidation;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Auth.PasskeySignIn;

internal sealed class PasskeySignInCommandValidator : AbstractValidator<PasskeySignInCommand>
{
    public PasskeySignInCommandValidator()
    {
        RuleFor(c => c.Credential)
            .NotEmpty()
                .WithMessage("Passkey credential is required.")
            .MaximumLength(UserOptions.MaxPasskeyCredentialLength)
                .WithMessage($"Passkey credential must not exceed {UserOptions.MaxPasskeyCredentialLength} characters.");

        RuleFor(c => c.State)
            .NotEmpty()
                .WithMessage("Passkey state is required.")
            .MaximumLength(UserOptions.MaxPasskeyStateLength)
                .WithMessage($"Passkey state must not exceed {UserOptions.MaxPasskeyStateLength} characters.");
    }
}
