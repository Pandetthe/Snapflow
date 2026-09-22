using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using AspNet.Security.OAuth.Apple;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Snapflow.Infrastructure.Auth.Cookies;
using Sustainsys.Saml2;
using Sustainsys.Saml2.AspNetCore2;
using Sustainsys.Saml2.Metadata;

namespace Snapflow.Infrastructure.Auth.External;

internal static class ExternalAuthenticationBuilderExtensions
{
    public static AuthenticationBuilder AddExternalProviders(this AuthenticationBuilder builder, AuthenticationProvidersOptions options)
    {
        if (options.Mode == AuthenticationMode.Local)
            return builder;

        if (options.Google.Enabled)
        {
            builder.AddGoogle(google =>
            {
                google.SignInScheme = IdentityConstants.ExternalScheme;
                google.CorrelationCookie.Name = AuthCookieNames.CorrelationPrefix;
                google.ClientId = options.Google.ClientId;
                google.ClientSecret = options.Google.ClientSecret;
                google.ClaimActions.MapJsonKey(ExternalProviderRegistry.EmailVerifiedClaimType, "email_verified");
            });
        }

        if (options.Microsoft.Enabled)
        {
            builder.AddMicrosoftAccount(microsoft =>
            {
                string tenant = Uri.EscapeDataString(options.Microsoft.TenantId);
                microsoft.SignInScheme = IdentityConstants.ExternalScheme;
                microsoft.CorrelationCookie.Name = AuthCookieNames.CorrelationPrefix;
                microsoft.ClientId = options.Microsoft.ClientId;
                microsoft.ClientSecret = options.Microsoft.ClientSecret;
                microsoft.AuthorizationEndpoint = $"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/authorize";
                microsoft.TokenEndpoint = $"https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token";
            });
        }

        if (options.Facebook.Enabled)
        {
            builder.AddFacebook(facebook =>
            {
                facebook.SignInScheme = IdentityConstants.ExternalScheme;
                facebook.CorrelationCookie.Name = AuthCookieNames.CorrelationPrefix;
                facebook.AppId = options.Facebook.ClientId;
                facebook.AppSecret = options.Facebook.ClientSecret;
            });
        }

        if (options.GitHub.Enabled)
        {
            builder.AddOAuth(ExternalProviderRegistry.GitHubScheme, options.GitHub.DisplayName, github =>
            {
                github.SignInScheme = IdentityConstants.ExternalScheme;
                github.CorrelationCookie.Name = AuthCookieNames.CorrelationPrefix;
                github.ClientId = options.GitHub.ClientId;
                github.ClientSecret = options.GitHub.ClientSecret;
                github.CallbackPath = "/signin-github";
                github.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                github.TokenEndpoint = "https://github.com/login/oauth/access_token";
                github.UserInformationEndpoint = "https://api.github.com/user";
                github.Scope.Add("read:user");
                github.Scope.Add("user:email");
                github.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                github.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                github.ClaimActions.MapJsonKey(ExternalProviderRegistry.GitHubLoginClaimType, "login");
                github.Events.OnCreatingTicket = AddGitHubUserAsync;
            });
        }

        if (options.Apple.Enabled)
        {
            builder.AddApple(apple =>
            {
                apple.SignInScheme = IdentityConstants.ExternalScheme;
                apple.CorrelationCookie.Name = AuthCookieNames.CorrelationPrefix;
                apple.CorrelationCookie.SameSite = SameSiteMode.None;
                apple.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
                apple.ClientId = options.Apple.ClientId;
                apple.TeamId = options.Apple.TeamId;
                apple.KeyId = options.Apple.KeyId;
                apple.ClientSecretExpiresAfter = TimeSpan.FromDays(options.Apple.ClientSecretExpiryDays);
                apple.UsePrivateKey(_ => new PhysicalFileInfo(new FileInfo(options.Apple.PrivateKeyPath)));
                apple.Scope.Add("name");
                apple.Scope.Add("email");
            });
        }

        foreach (OpenIdConnectProviderOptions provider in options.OpenIdConnect.Where(o => o.Enabled && !string.IsNullOrWhiteSpace(o.Scheme)))
        {
            string pathSuffix = provider.Scheme.ToLowerInvariant();

            builder.AddOpenIdConnect(provider.Scheme, provider.DisplayName, openId =>
            {
                openId.SignInScheme = IdentityConstants.ExternalScheme;
                openId.CorrelationCookie.Name = AuthCookieNames.CorrelationPrefix;
                openId.NonceCookie.Name = AuthCookieNames.NoncePrefix;
                openId.Authority = provider.Authority;
                openId.ClientId = provider.ClientId;
                openId.ClientSecret = string.IsNullOrEmpty(provider.ClientSecret) ? null : provider.ClientSecret;
                openId.RequireHttpsMetadata = provider.RequireHttpsMetadata;
                openId.ResponseType = OpenIdConnectResponseType.Code;
                openId.UsePkce = true;
                openId.GetClaimsFromUserInfoEndpoint = true;

                openId.CallbackPath = $"/signin-oidc-{pathSuffix}";
                openId.SignedOutCallbackPath = $"/signout-callback-oidc-{pathSuffix}";
                openId.RemoteSignOutPath = $"/signout-oidc-{pathSuffix}";

                openId.Scope.Clear();
                openId.Scope.Add("openid");
                openId.Scope.Add("profile");
                openId.Scope.Add("email");
                foreach (string scope in provider.Scopes)
                    openId.Scope.Add(scope);

                openId.ClaimActions.MapUniqueJsonKey(ExternalProviderRegistry.EmailVerifiedClaimType, "email_verified");
            });
        }

        if (options.Saml.Enabled)
        {
            builder.AddSaml2(Saml2Defaults.Scheme, options.Saml.DisplayName, saml2 =>
            {
                saml2.SignInScheme = IdentityConstants.ExternalScheme;
                saml2.SPOptions.EntityId = new EntityId(options.Saml.EntityId);

                if (!string.IsNullOrEmpty(options.Saml.SigningCertificatePath))
                {
                    saml2.SPOptions.ServiceCertificates.Add(X509CertificateLoader.LoadPkcs12FromFile(
                        options.Saml.SigningCertificatePath, options.Saml.SigningCertificatePassword));
                }

                saml2.IdentityProviders.Add(new IdentityProvider(new EntityId(options.Saml.IdpEntityId), saml2.SPOptions)
                {
                    MetadataLocation = options.Saml.IdpMetadataUrl,
                    LoadMetadata = true
                });
            });
        }

        return builder;
    }

    private static async Task AddGitHubUserAsync(OAuthCreatingTicketContext context)
    {
        using JsonDocument user = await GetGitHubJsonAsync(context, context.Options.UserInformationEndpoint);
        context.RunClaimActions(user.RootElement);

        using JsonDocument emails = await GetGitHubJsonAsync(context, "https://api.github.com/user/emails");
        string? email = emails.RootElement.EnumerateArray()
            .Where(e => e.GetProperty("verified").GetBoolean())
            .OrderByDescending(e => e.GetProperty("primary").GetBoolean())
            .Select(e => e.GetProperty("email").GetString())
            .FirstOrDefault(address => !string.IsNullOrWhiteSpace(address));

        if (email is not null && context.Identity is not null)
        {
            context.Identity.AddClaim(new Claim(ClaimTypes.Email, email));
            context.Identity.AddClaim(new Claim(ExternalProviderRegistry.EmailVerifiedClaimType, "true"));
        }
    }

    private static async Task<JsonDocument> GetGitHubJsonAsync(OAuthCreatingTicketContext context, string url)
    {
        CancellationToken cancellationToken = context.HttpContext.RequestAborted;

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
        request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Snapflow", "1.0"));

        using HttpResponseMessage response = await context.Backchannel.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using Stream content = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonDocument.ParseAsync(content, cancellationToken: cancellationToken);
    }
}
