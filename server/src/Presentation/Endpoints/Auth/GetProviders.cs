using Snapflow.Infrastructure.Auth.External;

namespace Snapflow.Presentation.Endpoints.Auth;

internal sealed class GetProviders : IEndpoint
{
    public sealed record ExternalProviderResponse(string Scheme, string DisplayName, string Type);

    public sealed record LdapProviderResponse(string DisplayName);

    public sealed record ProvidersResponse(
        bool PasswordSignIn,
        bool ExternalSignUp,
        LdapProviderResponse? Ldap,
        IReadOnlyList<ExternalProviderResponse> Providers,
        string? AutoRedirectScheme);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("auth/providers", (ExternalProviderRegistry registry) =>
        {
            var response = new ProvidersResponse(
                registry.PasswordAuthenticationEnabled,
                registry.ExternalSignUpEnabled,
                registry.LdapEnabled ? new LdapProviderResponse(registry.LdapDisplayName) : null,
                [.. registry.RedirectProviders.Select(p => new ExternalProviderResponse(p.Scheme, p.DisplayName, p.Type))],
                registry.AutoRedirectScheme);

            return Results.Ok(response);
        })
        .AllowAnonymous()
        .WithTags(EndpointTags.Auth)
        .Produces<ProvidersResponse>();
    }
}
