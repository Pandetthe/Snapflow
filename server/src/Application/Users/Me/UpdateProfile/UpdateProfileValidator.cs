using FluentValidation;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.UpdateProfile;

internal sealed class UpdateProfileValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileValidator()
    {
        RuleFor(c => c.UserName)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(UserOptions.MinUserNameLength)
                .WithMessage($"Username must be at least {UserOptions.MinUserNameLength} characters long.")
            .MaximumLength(UserOptions.MaxUserNameLength)
                .WithMessage($"Username must not exceed {UserOptions.MaxUserNameLength} characters.");
    }
}
