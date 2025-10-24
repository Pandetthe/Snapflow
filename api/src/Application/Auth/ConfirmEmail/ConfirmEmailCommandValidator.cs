using FluentValidation;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Auth.ConfirmEmail;

internal sealed class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(c => c.UserId)
            .GreaterThan(0).WithMessage("Invalid user identifier.");
        RuleFor(c => c.Code)
            .NotEmpty().WithMessage("Confirmation code must be provided.");
        RuleFor(c => c.ChangedEmail)
            .EmailAddress().When(c => !string.IsNullOrEmpty(c.ChangedEmail))
            .WithMessage("Changed email must be a valid email address.")
            .MaximumLength(UserOptions.MaxEmailLength).WithMessage("Changed email must not exceed 254 characters.");
    }
}