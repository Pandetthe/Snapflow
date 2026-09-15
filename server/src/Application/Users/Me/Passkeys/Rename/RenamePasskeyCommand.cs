using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Users.Me.Passkeys.Rename;

public sealed record RenamePasskeyCommand(string PasskeyId, string Name) : ICommand;
