using FluentValidation;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Auth.SignIn;

internal sealed class SignInCommandValidator : AbstractValidator<SignInCommand>
{
    public SignInCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.")
            .MaximumLength(UserOptions.MaxEmailLength)
            .WithMessage($"Email must not exceed {UserOptions.MaxEmailLength} characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(UserOptions.MinPasswordLength)
            .WithMessage($"Password must be at least {UserOptions.MinPasswordLength} characters long.")
            .MaximumLength(UserOptions.MaxPasswordLength)
            .WithMessage($"Password must not exceed {UserOptions.MaxPasswordLength} characters.")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .When(x => UserOptions.RequireLowercaseInPassword)
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .When(x => UserOptions.RequireUppercaseInPassword)
            .Matches(@"\d").WithMessage("Password must contain at least one number.")
            .When(x => UserOptions.RequireDigitInPassword)
            .Matches(@"[^\da-zA-Z]").WithMessage("Password must contain at least one symbol.")
            .When(x => UserOptions.RequireNonAlphanumeric);
    }
}