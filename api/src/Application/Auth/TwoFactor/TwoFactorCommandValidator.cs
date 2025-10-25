using FluentValidation;

namespace Snapflow.Application.Auth.TwoFactor;

internal sealed class TwoFactorCommandValidator : AbstractValidator<TwoFactorCommand>
{
    public TwoFactorCommandValidator()
    {
    }
}