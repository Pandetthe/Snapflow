using FluentValidation;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.Passkeys.Remove;

internal sealed class RemovePasskeyValidator : AbstractValidator<RemovePasskeyCommand>
{
    public RemovePasskeyValidator()
    {
        RuleFor(c => c.PasskeyId)
            .NotEmpty()
                .WithMessage("Passkey id is required.")
            .MaximumLength(UserOptions.MaxPasskeyIdLength)
                .WithMessage($"Passkey id must not exceed {UserOptions.MaxPasskeyIdLength} characters.");
    }
}
