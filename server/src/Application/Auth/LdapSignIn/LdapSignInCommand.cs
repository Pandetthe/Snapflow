using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Auth.LdapSignIn;

public sealed record LdapSignInCommand(
    string UserName,
    string Password,
    bool? UseCookies,
    bool? UseSessionCookies) : ICommand;
