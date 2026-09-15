namespace Snapflow.Application.Abstractions.Identity;

public sealed record ExternalIdentity(
    string Provider,
    string ProviderKey,
    string ProviderDisplayName,
    string? Email,
    bool EmailVerified,
    string? Name);

public sealed record ExternalSignInTicket(ExternalIdentity Identity, bool IsPersistent);
