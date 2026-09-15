using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Users.Me.TwoFactor.SetupAuthenticator;

public sealed record SetupAuthenticatorCommand : ICommand<SetupAuthenticatorResponse>;
