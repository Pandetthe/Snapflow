using System.DirectoryServices.Protocols;
using System.Globalization;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Infrastructure.Auth.External;

internal sealed class LdapAuthenticator(
    IOptions<AuthenticationProvidersOptions> options,
    ExternalProviderRegistry registry,
    ILogger<LdapAuthenticator> logger) : ILdapAuthenticator
{
    private const int InvalidCredentials = 49;

    public bool IsEnabled => registry.LdapEnabled;

    public Task<Result<ExternalIdentity>> AuthenticateAsync(string userName, string password, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled || string.IsNullOrWhiteSpace(userName) || string.IsNullOrEmpty(password))
            return Task.FromResult(Result.Failure<ExternalIdentity>(UserErrors.SignInFailed));

        return Task.Run(() => Authenticate(options.Value.Ldap, userName, password), cancellationToken);
    }

    private Result<ExternalIdentity> Authenticate(LdapProviderOptions ldap, string userName, string password)
    {
        try
        {
            SearchResultEntry? entry = FindUser(ldap, userName);
            if (entry is null)
                return Result.Failure<ExternalIdentity>(UserErrors.SignInFailed);

            try
            {
                using LdapConnection userConnection = Connect(ldap);
                userConnection.Bind(new NetworkCredential(entry.DistinguishedName, password));
            }
            catch (LdapException ex) when (ex.ErrorCode == InvalidCredentials)
            {
                return Result.Failure<ExternalIdentity>(UserErrors.SignInFailed);
            }

            string? id = ReadId(entry, ldap.IdAttribute);
            if (id is null)
            {
                logger.LogWarning("LDAP entry {DistinguishedName} has no {IdAttribute} attribute.", entry.DistinguishedName, ldap.IdAttribute);
                return Result.Failure<ExternalIdentity>(AuthenticationErrors.ExternalSignInFailed);
            }

            string? email = ReadString(entry, ldap.EmailAttribute);

            return Result.Success(new ExternalIdentity(
                ExternalProviderRegistry.LdapProvider,
                id,
                ldap.DisplayName,
                email,
                ldap.TrustEmail && email is not null,
                ReadString(entry, ldap.NameAttribute)));
        }
        catch (Exception ex) when (ex is LdapException or DirectoryOperationException)
        {
            logger.LogError(ex, "LDAP sign-in against {Host} failed.", ldap.Host);
            return Result.Failure<ExternalIdentity>(AuthenticationErrors.ExternalSignInFailed);
        }
    }

    private SearchResultEntry? FindUser(LdapProviderOptions ldap, string userName)
    {
        using LdapConnection connection = Connect(ldap);

        if (string.IsNullOrEmpty(ldap.BindDn))
        {
            connection.AuthType = AuthType.Anonymous;
            connection.Bind();
        }
        else
        {
            connection.Bind(new NetworkCredential(ldap.BindDn, ldap.BindPassword));
        }

        string filter = string.Format(CultureInfo.InvariantCulture, ldap.UserFilter, LdapFilter.Escape(userName));
        var request = new SearchRequest(ldap.SearchBase, filter, SearchScope.Subtree, ldap.IdAttribute, ldap.EmailAttribute, ldap.NameAttribute);
        var response = (SearchResponse)connection.SendRequest(request);

        if (response.Entries.Count == 1)
            return response.Entries[0];

        if (response.Entries.Count > 1)
            logger.LogWarning("LDAP user filter matched {Count} entries for one user name.", response.Entries.Count);

        return null;
    }

    private static LdapConnection Connect(LdapProviderOptions ldap)
    {
        var connection = new LdapConnection(new LdapDirectoryIdentifier(ldap.Host, ldap.Port))
        {
            AuthType = AuthType.Basic,
            Timeout = TimeSpan.FromSeconds(ldap.TimeoutSeconds)
        };

        try
        {
            connection.SessionOptions.ProtocolVersion = 3;
            connection.SessionOptions.ReferralChasing = ReferralChasingOptions.None;

            if (ldap.Security == LdapSecurity.Ssl)
                connection.SessionOptions.SecureSocketLayer = true;
            else if (ldap.Security == LdapSecurity.StartTls)
                connection.SessionOptions.StartTransportLayerSecurity(null);

            return connection;
        }
        catch
        {
            connection.Dispose();
            throw;
        }
    }

    private static DirectoryAttribute? FindAttribute(SearchResultEntry entry, string name)
    {
        foreach (string attributeName in entry.Attributes.AttributeNames)
        {
            if (string.Equals(attributeName, name, StringComparison.OrdinalIgnoreCase))
                return entry.Attributes[attributeName];
        }

        return null;
    }

    private static string? ReadString(SearchResultEntry entry, string name) =>
        FindAttribute(entry, name) is { Count: > 0 } attribute && attribute.GetValues(typeof(string))[0] is string { Length: > 0 } value
            ? value
            : null;

    private static string? ReadId(SearchResultEntry entry, string name)
    {
        if (name.Equals("objectGUID", StringComparison.OrdinalIgnoreCase))
        {
            return FindAttribute(entry, name) is { Count: > 0 } attribute && attribute.GetValues(typeof(byte[]))[0] is byte[] { Length: 16 } bytes
                ? new Guid(bytes).ToString()
                : null;
        }

        return ReadString(entry, name);
    }
}
