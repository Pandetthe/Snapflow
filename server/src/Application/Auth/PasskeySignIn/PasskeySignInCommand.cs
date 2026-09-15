using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Auth.PasskeySignIn;

public sealed record PasskeySignInCommand(
    string Credential,
    string State,
    bool? UseCookies,
    bool? UseSessionCookies) : ICommand;
