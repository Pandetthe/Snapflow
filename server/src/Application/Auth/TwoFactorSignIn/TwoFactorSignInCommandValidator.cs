using FluentValidation;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Auth.TwoFactorSignIn;

internal sealed class TwoFactorSignInCommandValidator : AbstractValidator<TwoFactorSignInCommand>
{
    public TwoFactorSignInCommandValidator()
    {
        RuleFor(c => c.Code)
            .NotEmpty().When(c => string.IsNullOrWhiteSpace(c.RecoveryCode) && string.IsNullOrWhiteSpace(c.PasskeyCredential))
                .WithMessage("Enter a code from your authenticator app, a recovery code or use a passkey.")
            .MaximumLength(UserOptions.MaxTwoFactorCodeLength)
                .WithMessage($"Code must not exceed {UserOptions.MaxTwoFactorCodeLength} characters.");

        RuleFor(c => c.RecoveryCode)
            .MaximumLength(UserOptions.MaxTwoFactorCodeLength)
                .WithMessage($"Recovery code must not exceed {UserOptions.MaxTwoFactorCodeLength} characters.");

        RuleFor(c => c.PasskeyCredential)
            .MaximumLength(UserOptions.MaxPasskeyCredentialLength)
                .WithMessage($"Passkey credential must not exceed {UserOptions.MaxPasskeyCredentialLength} characters.");

        RuleFor(c => c.PasskeyState)
            .NotEmpty().When(c => !string.IsNullOrWhiteSpace(c.PasskeyCredential))
                .WithMessage("Passkey state is required when a passkey is used.")
            .MaximumLength(UserOptions.MaxPasskeyStateLength)
                .WithMessage($"Passkey state must not exceed {UserOptions.MaxPasskeyStateLength} characters.");

        RuleFor(c => c.TwoFactorToken)
            .MaximumLength(UserOptions.MaxTwoFactorTokenLength)
                .WithMessage($"Two-factor token must not exceed {UserOptions.MaxTwoFactorTokenLength} characters.");
    }
}
