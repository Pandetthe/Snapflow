using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.Extensions.Options;
using Snapflow.Application.Abstractions.Identity;
using Sustainsys.Saml2.AspNetCore2;

namespace Snapflow.Infrastructure.Auth.External;

public sealed record ExternalProvider(
    string Scheme,
    string DisplayName,
    string Type,
    bool TrustEmail,
    IReadOnlyList<string> EmailClaimTypes,
    IReadOnlyList<string> NameClaimTypes);

public sealed class ExternalProviderRegistry : IAuthenticationSettings
{
    public const string EmailVerifiedClaimType = "email_verified";
    public const string PersistentItemKey = "snapflow.persistent";
    public const string LdapProvider = "Ldap";
    public const string GitHubScheme = "GitHub";
    public const string GitHubLoginClaimType = "urn:github:login";

    private static readonly string[] DefaultEmailClaimTypes = [ClaimTypes.Email, "email"];
    private static readonly string[] DefaultNameClaimTypes = [ClaimTypes.Name, "name"];

    private readonly AuthenticationProvidersOptions _options;
    private readonly Dictionary<string, ExternalProvider> _byScheme;

    public ExternalProviderRegistry(IOptions<AuthenticationProvidersOptions> options)
    {
        _options = options.Value;
        RedirectProviders = _options.Mode == AuthenticationMode.Local ? [] : [.. BuildRedirectProviders(_options)];
        _byScheme = RedirectProviders.ToDictionary(p => p.Scheme, StringComparer.Ordinal);
    }

    public IReadOnlyList<ExternalProvider> RedirectProviders { get; }

    public bool LdapEnabled => _options.Mode != AuthenticationMode.Local && _options.Ldap.Enabled;

    public string LdapDisplayName => _options.Ldap.DisplayName;

    public bool PasswordAuthenticationEnabled => _options.Mode != AuthenticationMode.External;

    public bool ExternalSignUpEnabled => _options.AllowExternalSignUp;

    public string? AutoRedirectScheme =>
        _options.AutoRedirect && _options.Mode == AuthenticationMode.External && RedirectProviders.Count == 1 && !LdapEnabled
            ? RedirectProviders[0].Scheme
            : null;

    public ExternalProvider? FindRedirectProvider(string scheme) => _byScheme.GetValueOrDefault(scheme);

    private static IEnumerable<ExternalProvider> BuildRedirectProviders(AuthenticationProvidersOptions options)
    {
        if (options.Google.Enabled)
            yield return new ExternalProvider(GoogleDefaults.AuthenticationScheme, options.Google.DisplayName, "google",
                options.Google.TrustEmail, DefaultEmailClaimTypes, DefaultNameClaimTypes);

        if (options.Microsoft.Enabled)
            yield return new ExternalProvider(MicrosoftAccountDefaults.AuthenticationScheme, options.Microsoft.DisplayName, "microsoft",
                options.Microsoft.TrustEmail, DefaultEmailClaimTypes, DefaultNameClaimTypes);

        if (options.Facebook.Enabled)
            yield return new ExternalProvider(FacebookDefaults.AuthenticationScheme, options.Facebook.DisplayName, "facebook",
                options.Facebook.TrustEmail, DefaultEmailClaimTypes, DefaultNameClaimTypes);

        if (options.GitHub.Enabled)
            yield return new ExternalProvider(GitHubScheme, options.GitHub.DisplayName, "github",
                options.GitHub.TrustEmail, DefaultEmailClaimTypes, [ClaimTypes.Name, GitHubLoginClaimType]);

        foreach (OpenIdConnectProviderOptions provider in options.OpenIdConnect.Where(o => o.Enabled))
            yield return new ExternalProvider(provider.Scheme, provider.DisplayName, "oidc",
                provider.TrustEmail, DefaultEmailClaimTypes, DefaultNameClaimTypes);

        if (options.Saml.Enabled)
            yield return new ExternalProvider(Saml2Defaults.Scheme, options.Saml.DisplayName, "saml", options.Saml.TrustEmail,
                [options.Saml.EmailAttribute, .. DefaultEmailClaimTypes],
                [options.Saml.NameAttribute, .. DefaultNameClaimTypes]);
    }
}
