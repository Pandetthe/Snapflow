using FluentValidation;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Auth.TwoFactorSignIn;

internal sealed class TwoFactorSignInCommandValidator : AbstractValidator<TwoFactorSignInCommand>
{
    public TwoFactorSignInCommandValidator()
    {
        RuleFor(c => c.Code)
            .NotEmpty().When(c => string.IsNullOrWhiteSpace(c.RecoveryCode))
                .WithMessage("Enter a code from your authenticator app or a recovery code.")
            .MaximumLength(UserOptions.MaxTwoFactorCodeLength)
                .WithMessage($"Code must not exceed {UserOptions.MaxTwoFactorCodeLength} characters.");

        RuleFor(c => c.RecoveryCode)
            .MaximumLength(UserOptions.MaxTwoFactorCodeLength)
                .WithMessage($"Recovery code must not exceed {UserOptions.MaxTwoFactorCodeLength} characters.");

        RuleFor(c => c.TwoFactorToken)
            .MaximumLength(UserOptions.MaxTwoFactorTokenLength)
                .WithMessage($"Two-factor token must not exceed {UserOptions.MaxTwoFactorTokenLength} characters.");
    }
}
