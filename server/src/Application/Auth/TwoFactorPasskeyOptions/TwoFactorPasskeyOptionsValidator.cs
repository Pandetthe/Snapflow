using FluentValidation;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Auth.TwoFactorPasskeyOptions;

internal sealed class TwoFactorPasskeyOptionsValidator : AbstractValidator<TwoFactorPasskeyOptionsCommand>
{
    public TwoFactorPasskeyOptionsValidator()
    {
        RuleFor(c => c.TwoFactorToken)
            .MaximumLength(UserOptions.MaxTwoFactorTokenLength)
                .WithMessage($"Two-factor token must not exceed {UserOptions.MaxTwoFactorTokenLength} characters.");
    }
}
