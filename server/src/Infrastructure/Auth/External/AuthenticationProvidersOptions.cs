using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Snapflow.Infrastructure.Auth.External;

public enum AuthenticationMode
{
    Local,

    Mixed,

    External
}

public enum LdapSecurity
{
    None,
    StartTls,
    Ssl
}

public sealed partial class AuthenticationProvidersOptions : IValidatableObject
{
    public const string SectionName = "Authentication";

    private static readonly string[] ReservedSchemes = ["Google", "Microsoft", "Facebook", "GitHub", "Apple", "Saml2", "Ldap", "callback"];

    public AuthenticationMode Mode { get; init; } = AuthenticationMode.Local;

    public bool AllowExternalSignUp { get; init; } = true;

    public bool AutoRedirect { get; init; }

    public OAuthProviderOptions Google { get; init; } = new() { DisplayName = "Google", TrustEmail = true };

    public MicrosoftProviderOptions Microsoft { get; init; } = new() { DisplayName = "Microsoft" };

    public OAuthProviderOptions Facebook { get; init; } = new() { DisplayName = "Facebook", TrustEmail = true };

    public OAuthProviderOptions GitHub { get; init; } = new() { DisplayName = "GitHub" };

    public AppleProviderOptions Apple { get; init; } = new();

    public List<OpenIdConnectProviderOptions> OpenIdConnect { get; init; } = [];

    public SamlProviderOptions Saml { get; init; } = new();

    public LdapProviderOptions Ldap { get; init; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        int redirectProviders = new[] { Google.Enabled, Microsoft.Enabled, Facebook.Enabled, GitHub.Enabled, Apple.Enabled, Saml.Enabled }.Count(enabled => enabled)
            + OpenIdConnect.Count(o => o.Enabled);

        if (Mode == AuthenticationMode.External && redirectProviders == 0 && !Ldap.Enabled)
            yield return new ValidationResult("Authentication mode External needs at least one enabled provider.", [nameof(Mode)]);

        if (AutoRedirect && (Mode != AuthenticationMode.External || redirectProviders != 1 || Ldap.Enabled))
            yield return new ValidationResult("AutoRedirect needs mode External with exactly one sign-in provider and LDAP disabled.", [nameof(AutoRedirect)]);

        foreach ((string name, OAuthProviderOptions provider) in new[] { ("Google", Google), ("Microsoft", Microsoft), ("Facebook", (OAuthProviderOptions)Facebook), ("GitHub", GitHub) })
        {
            if (provider.Enabled && (string.IsNullOrWhiteSpace(provider.ClientId) || string.IsNullOrWhiteSpace(provider.ClientSecret)))
                yield return new ValidationResult($"{name} needs ClientId and ClientSecret when enabled.", [name]);
        }

        if (Apple.Enabled)
        {
            if (string.IsNullOrWhiteSpace(Apple.ClientId) || string.IsNullOrWhiteSpace(Apple.TeamId) || string.IsNullOrWhiteSpace(Apple.KeyId))
                yield return new ValidationResult("Apple needs ClientId, TeamId and KeyId when enabled.", [nameof(Apple)]);
            if (string.IsNullOrWhiteSpace(Apple.PrivateKeyPath))
                yield return new ValidationResult("Apple needs PrivateKeyPath pointing at the .p8 key file when enabled.", [nameof(Apple)]);
            if (Apple.ClientSecretExpiryDays is < 1 or > 180)
                yield return new ValidationResult("Apple ClientSecretExpiryDays must be between 1 and 180.", [nameof(Apple)]);
        }

        HashSet<string> schemes = new(ReservedSchemes, StringComparer.OrdinalIgnoreCase);
        foreach (OpenIdConnectProviderOptions provider in OpenIdConnect.Where(o => o.Enabled))
        {
            if (string.IsNullOrWhiteSpace(provider.Scheme) || !SchemePattern().IsMatch(provider.Scheme))
                yield return new ValidationResult($"OpenIdConnect scheme '{provider.Scheme}' must consist of letters, digits, '-' and '_'.", [nameof(OpenIdConnect)]);
            else if (!schemes.Add(provider.Scheme))
                yield return new ValidationResult($"OpenIdConnect scheme '{provider.Scheme}' is used twice or reserved.", [nameof(OpenIdConnect)]);

            if (string.IsNullOrWhiteSpace(provider.DisplayName) || string.IsNullOrWhiteSpace(provider.Authority) || string.IsNullOrWhiteSpace(provider.ClientId))
                yield return new ValidationResult($"OpenIdConnect provider '{provider.Scheme}' needs DisplayName, Authority and ClientId.", [nameof(OpenIdConnect)]);
        }

        if (Saml.Enabled && (string.IsNullOrWhiteSpace(Saml.EntityId) || string.IsNullOrWhiteSpace(Saml.IdpEntityId) || string.IsNullOrWhiteSpace(Saml.IdpMetadataUrl)))
            yield return new ValidationResult("Saml needs EntityId, IdpEntityId and IdpMetadataUrl when enabled.", [nameof(Saml)]);

        if (Ldap.Enabled)
        {
            if (string.IsNullOrWhiteSpace(Ldap.Host) || string.IsNullOrWhiteSpace(Ldap.SearchBase) || string.IsNullOrWhiteSpace(Ldap.IdAttribute))
                yield return new ValidationResult("Ldap needs Host, SearchBase and IdAttribute when enabled.", [nameof(Ldap)]);
            if (!Ldap.UserFilter.Contains("{0}", StringComparison.Ordinal))
                yield return new ValidationResult("Ldap UserFilter must contain {0} where the user name goes.", [nameof(Ldap)]);
            if (Ldap.TimeoutSeconds is < 1 or > 300)
                yield return new ValidationResult("Ldap TimeoutSeconds must be between 1 and 300.", [nameof(Ldap)]);
        }
    }

    [GeneratedRegex("^[A-Za-z0-9_-]+$")]
    private static partial Regex SchemePattern();
}

public class OAuthProviderOptions
{
    public bool Enabled { get; init; }

    public string ClientId { get; init; } = "";

    public string ClientSecret { get; init; } = "";

    public string DisplayName { get; init; } = "";

    public bool TrustEmail { get; init; }
}

public sealed class AppleProviderOptions
{
    public bool Enabled { get; init; }

    public string ClientId { get; init; } = "";

    public string TeamId { get; init; } = "";

    public string KeyId { get; init; } = "";

    public string PrivateKeyPath { get; init; } = "";

    public string DisplayName { get; init; } = "Apple";

    public bool TrustEmail { get; init; } = true;

    public int ClientSecretExpiryDays { get; init; } = 180;
}

public sealed class MicrosoftProviderOptions : OAuthProviderOptions
{
    public string TenantId { get; init; } = "common";
}

public sealed class OpenIdConnectProviderOptions
{
    public bool Enabled { get; init; } = true;

    public string Scheme { get; init; } = "";

    public string DisplayName { get; init; } = "";

    public string Authority { get; init; } = "";

    public string ClientId { get; init; } = "";

    public string ClientSecret { get; init; } = "";

    public string[] Scopes { get; init; } = [];

    public bool TrustEmail { get; init; }

    public bool RequireHttpsMetadata { get; init; } = true;
}

public sealed class SamlProviderOptions
{
    public bool Enabled { get; init; }

    public string DisplayName { get; init; } = "Single sign-on";

    public string EntityId { get; init; } = "";

    public string IdpEntityId { get; init; } = "";

    public string IdpMetadataUrl { get; init; } = "";

    public string? SigningCertificatePath { get; init; }

    public string? SigningCertificatePassword { get; init; }

    public string EmailAttribute { get; init; } = ClaimTypes.Email;

    public string NameAttribute { get; init; } = ClaimTypes.Name;

    public bool TrustEmail { get; init; } = true;
}

public sealed class LdapProviderOptions
{
    public bool Enabled { get; init; }

    public string DisplayName { get; init; } = "Company account";

    public string Host { get; init; } = "";

    public int Port { get; init; } = 389;

    public LdapSecurity Security { get; init; } = LdapSecurity.StartTls;

    public string? BindDn { get; init; }

    public string? BindPassword { get; init; }

    public string SearchBase { get; init; } = "";

    public string UserFilter { get; init; } = "(&(objectClass=person)(|(uid={0})(mail={0})))";

    public string IdAttribute { get; init; } = "entryUUID";

    public string EmailAttribute { get; init; } = "mail";

    public string NameAttribute { get; init; } = "cn";

    public bool TrustEmail { get; init; } = true;

    public int TimeoutSeconds { get; init; } = 10;
}
