using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Users.Me.Passkeys.Add;

public sealed record AddPasskeyCommand(string Credential, string State, string? Name) : ICommand<PasskeyDetails>;
