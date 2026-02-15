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
    }
}