using FluentValidation;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.TwoFactor.Enable;

internal sealed class EnableTwoFactorValidator : AbstractValidator<EnableTwoFactorCommand>
{
    public EnableTwoFactorValidator()
    {
        RuleFor(c => c.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(UserOptions.MaxTwoFactorCodeLength)
                .WithMessage($"Code must not exceed {UserOptions.MaxTwoFactorCodeLength} characters.");
    }
}
