using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Users.Me.Passkeys.CreateOptions;

public sealed record CreatePasskeyOptionsCommand : ICommand<PasskeyChallenge>;
