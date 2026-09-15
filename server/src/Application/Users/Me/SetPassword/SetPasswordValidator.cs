using FluentValidation;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.SetPassword;

internal sealed class SetPasswordValidator : AbstractValidator<SetPasswordCommand>
{
    public SetPasswordValidator()
    {
        RuleFor(c => c.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(UserOptions.MinPasswordLength)
                .WithMessage($"Password must be at least {UserOptions.MinPasswordLength} characters long.")
            .MaximumLength(UserOptions.MaxPasswordLength)
                .WithMessage($"Password must not exceed {UserOptions.MaxPasswordLength} characters.");
    }
}
