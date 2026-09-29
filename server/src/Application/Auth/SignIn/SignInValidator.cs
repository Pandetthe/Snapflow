using FluentValidation;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Auth.SignIn;

internal sealed class SignInValidator : AbstractValidator<SignInCommand>
{
    public SignInValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.")
            .MaximumLength(UserOptions.MaxEmailLength)
            .WithMessage($"Email must not exceed {UserOptions.MaxEmailLength} characters.");

        RuleFor(c => c.RememberDeviceToken)
            .MaximumLength(UserOptions.MaxTwoFactorTokenLength)
            .WithMessage($"Remember-device token must not exceed {UserOptions.MaxTwoFactorTokenLength} characters.");
    }
}