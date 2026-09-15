using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Users.Me.Passkeys.Remove;

public sealed record RemovePasskeyCommand(string PasskeyId) : ICommand;
