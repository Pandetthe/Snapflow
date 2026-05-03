using FluentValidation;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.ChangePassword;

internal sealed class ChangePasswordValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordValidator()
    {
        RuleFor(c => c.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");

        RuleFor(c => c.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(UserOptions.MinPasswordLength)
                .WithMessage($"Password must be at least {UserOptions.MinPasswordLength} characters long.")
            .MaximumLength(UserOptions.MaxPasswordLength)
                .WithMessage($"Password must not exceed {UserOptions.MaxPasswordLength} characters.");
    }
}
