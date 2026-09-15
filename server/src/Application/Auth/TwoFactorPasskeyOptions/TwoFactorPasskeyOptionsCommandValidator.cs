using FluentValidation;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Auth.TwoFactorPasskeyOptions;

internal sealed class TwoFactorPasskeyOptionsCommandValidator : AbstractValidator<TwoFactorPasskeyOptionsCommand>
{
    public TwoFactorPasskeyOptionsCommandValidator()
    {
        RuleFor(c => c.TwoFactorToken)
            .MaximumLength(UserOptions.MaxTwoFactorTokenLength)
                .WithMessage($"Two-factor token must not exceed {UserOptions.MaxTwoFactorTokenLength} characters.");
    }
}
