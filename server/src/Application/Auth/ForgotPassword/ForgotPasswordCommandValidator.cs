using FluentValidation;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Auth.ForgotPassword;

internal sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(UserOptions.MaxEmailLength).WithMessage("Email must not exceed 254 characters.");
    }
}