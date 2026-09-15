using Snapflow.Common;

namespace Snapflow.Application.Abstractions.Identity;

public interface ILdapAuthenticator
{
    bool IsEnabled { get; }

    Task<Result<ExternalIdentity>> AuthenticateAsync(string userName, string password, CancellationToken cancellationToken = default);
}
