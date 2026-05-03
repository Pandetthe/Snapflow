using FluentValidation;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.UpdateAvatar;

internal sealed class UpdateAvatarValidator : AbstractValidator<UpdateAvatarCommand>
{
    public UpdateAvatarValidator()
    {
        RuleFor(c => c.AvatarData)
            .NotNull().When(c => c.AvatarType == AvatarType.Uploaded)
            .WithMessage("Avatar file is required when type is Uploaded.");
    }
}
