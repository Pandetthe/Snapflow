using FluentValidation;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Auth.ResetPassword;

internal sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.")
            .MaximumLength(UserOptions.MaxEmailLength)
            .WithMessage("Email must not exceed 254 characters.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(UserOptions.MinPasswordLength)
            .WithMessage("New password must be at least 8 characters long.")
            .MaximumLength(UserOptions.MaxPasswordLength)
            .WithMessage("New password must not exceed 64 characters.")
            .Matches(@"[a-z]").WithMessage("New password must contain at least one lowercase letter.")
            .When(x => UserOptions.RequireLowercaseInPassword)
            .Matches(@"[A-Z]").WithMessage("New password must contain at least one uppercase letter.")
            .When(x => UserOptions.RequireUppercaseInPassword)
            .Matches(@"\d").WithMessage("New password must contain at least one number.")
            .When(x => UserOptions.RequireDigitInPassword)
            .Matches(@"[^\da-zA-Z]").WithMessage("New password must contain at least one symbol.")
            .When(x => UserOptions.RequireNonAlphanumeric);
    }
}