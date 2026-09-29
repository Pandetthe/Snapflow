using FluentValidation;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Auth.ResendConfirmationEmail;

internal sealed class ResendConfirmationEmailValidator : AbstractValidator<ResendConfirmationEmailCommand>
{
    public ResendConfirmationEmailValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.")
            .MaximumLength(UserOptions.MaxEmailLength)
            .WithMessage($"Email must not exceed {UserOptions.MaxEmailLength} characters.");
    }
}