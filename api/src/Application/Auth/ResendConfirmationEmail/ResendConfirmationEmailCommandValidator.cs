using FluentValidation;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Auth.ResendConfirmationEmail;

internal sealed class ResendConfirmationEmailCommandValidator : AbstractValidator<ResendConfirmationEmailCommand>
{
    public ResendConfirmationEmailCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.")
            .MaximumLength(UserOptions.MaxEmailLength)
            .WithMessage("Email must not exceed 254 characters.");
    }
}