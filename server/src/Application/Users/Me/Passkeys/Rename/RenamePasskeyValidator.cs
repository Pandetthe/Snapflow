using FluentValidation;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.Passkeys.Rename;

internal sealed class RenamePasskeyValidator : AbstractValidator<RenamePasskeyCommand>
{
    public RenamePasskeyValidator()
    {
        RuleFor(c => c.PasskeyId)
            .NotEmpty()
                .WithMessage("Passkey id is required.")
            .MaximumLength(UserOptions.MaxPasskeyIdLength)
                .WithMessage($"Passkey id must not exceed {UserOptions.MaxPasskeyIdLength} characters.");

        RuleFor(c => c.Name)
            .Must(name => !string.IsNullOrWhiteSpace(name))
                .WithMessage("Name is required.")
            .MaximumLength(UserOptions.MaxPasskeyNameLength)
                .WithMessage($"Name must not exceed {UserOptions.MaxPasskeyNameLength} characters.");
    }
}
