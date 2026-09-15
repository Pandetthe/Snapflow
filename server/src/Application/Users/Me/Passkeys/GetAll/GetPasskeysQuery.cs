using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Users.Me.Passkeys.GetAll;

public sealed record GetPasskeysQuery : IQuery<IReadOnlyList<PasskeyDetails>>;
