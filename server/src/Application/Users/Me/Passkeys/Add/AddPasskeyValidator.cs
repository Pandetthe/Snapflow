using FluentValidation;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.Passkeys.Add;

internal sealed class AddPasskeyValidator : AbstractValidator<AddPasskeyCommand>
{
    public AddPasskeyValidator()
    {
        RuleFor(c => c.Credential)
            .NotEmpty()
                .WithMessage("Passkey credential is required.")
            .MaximumLength(UserOptions.MaxPasskeyCredentialLength)
                .WithMessage($"Passkey credential must not exceed {UserOptions.MaxPasskeyCredentialLength} characters.");

        RuleFor(c => c.State)
            .NotEmpty()
                .WithMessage("Passkey state is required.")
            .MaximumLength(UserOptions.MaxPasskeyStateLength)
                .WithMessage($"Passkey state must not exceed {UserOptions.MaxPasskeyStateLength} characters.");

        RuleFor(c => c.Name)
            .MaximumLength(UserOptions.MaxPasskeyNameLength)
                .WithMessage($"Name must not exceed {UserOptions.MaxPasskeyNameLength} characters.");
    }
}
