using FluentValidation;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.TwoFactor.RegenerateRecoveryCodes;

internal sealed class RegenerateRecoveryCodesValidator : AbstractValidator<RegenerateRecoveryCodesCommand>
{
    public RegenerateRecoveryCodesValidator()
    {
        RuleFor(c => c.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(UserOptions.MaxTwoFactorCodeLength)
                .WithMessage($"Code must not exceed {UserOptions.MaxTwoFactorCodeLength} characters.");
    }
}
