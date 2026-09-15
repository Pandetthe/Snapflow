using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Snapflow.Infrastructure.Auth.External;

namespace Snapflow.UnitTests.Infrastructure;

public sealed class AuthenticationProvidersOptionsTests
{
    private static List<ValidationResult> Validate(AuthenticationProvidersOptions options) =>
        [.. options.Validate(new ValidationContext(options))];

    [Fact]
    public void Validate_Should_Pass_When_Defaults()
    {
        Validate(new AuthenticationProvidersOptions()).Should().BeEmpty();
    }

    [Fact]
    public void Validate_Should_Fail_When_ExternalModeWithoutProviders()
    {
        var options = new AuthenticationProvidersOptions { Mode = AuthenticationMode.External };

        Validate(options).Should().ContainSingle();
    }

    [Fact]
    public void Validate_Should_Fail_When_EnabledProviderMissesSecret()
    {
        var options = new AuthenticationProvidersOptions
        {
            Google = new OAuthProviderOptions { Enabled = true, ClientId = "id" }
        };

        Validate(options).Should().ContainSingle();
    }

    [Theory]
    [InlineData("Google")]
    [InlineData("callback")]
    [InlineData("has space")]
    public void Validate_Should_Fail_When_OpenIdConnectSchemeReservedOrInvalid(string scheme)
    {
        var options = new AuthenticationProvidersOptions
        {
            OpenIdConnect = [new OpenIdConnectProviderOptions { Scheme = scheme, DisplayName = "Corp", Authority = "https://id.example.com", ClientId = "id" }]
        };

        Validate(options).Should().ContainSingle();
    }

    [Fact]
    public void Validate_Should_Fail_When_LdapFilterHasNoPlaceholder()
    {
        var options = new AuthenticationProvidersOptions
        {
            Ldap = new LdapProviderOptions { Enabled = true, Host = "ldap", SearchBase = "dc=example,dc=com", UserFilter = "(uid=jan)" }
        };

        Validate(options).Should().ContainSingle();
    }

    [Fact]
    public void Registry_Should_OfferNoProviders_When_LocalMode()
    {
        var registry = new ExternalProviderRegistry(Options.Create(new AuthenticationProvidersOptions
        {
            Mode = AuthenticationMode.Local,
            Google = new OAuthProviderOptions { Enabled = true, ClientId = "id", ClientSecret = "secret" },
            Ldap = new LdapProviderOptions { Enabled = true }
        }));

        registry.RedirectProviders.Should().BeEmpty();
        registry.LdapEnabled.Should().BeFalse();
        registry.PasswordAuthenticationEnabled.Should().BeTrue();
    }

    [Fact]
    public void Registry_Should_DisablePasswords_When_ExternalMode()
    {
        var registry = new ExternalProviderRegistry(Options.Create(new AuthenticationProvidersOptions
        {
            Mode = AuthenticationMode.External,
            OpenIdConnect = [new OpenIdConnectProviderOptions { Scheme = "corp", DisplayName = "Corp", Authority = "https://id.example.com", ClientId = "id" }]
        }));

        registry.PasswordAuthenticationEnabled.Should().BeFalse();
        registry.FindRedirectProvider("corp").Should().NotBeNull();
        registry.FindRedirectProvider("Google").Should().BeNull();
    }

    [Fact]
    public void Validate_Should_Fail_When_AutoRedirectWithSeveralProviders()
    {
        var options = new AuthenticationProvidersOptions
        {
            Mode = AuthenticationMode.External,
            AutoRedirect = true,
            Google = new OAuthProviderOptions { Enabled = true, ClientId = "id", ClientSecret = "secret" },
            GitHub = new OAuthProviderOptions { Enabled = true, ClientId = "id", ClientSecret = "secret" }
        };

        Validate(options).Should().ContainSingle();
    }

    [Fact]
    public void Validate_Should_Fail_When_AutoRedirectOutsideExternalMode()
    {
        var options = new AuthenticationProvidersOptions
        {
            Mode = AuthenticationMode.Mixed,
            AutoRedirect = true,
            GitHub = new OAuthProviderOptions { Enabled = true, ClientId = "id", ClientSecret = "secret" }
        };

        Validate(options).Should().ContainSingle();
    }

    [Fact]
    public void Validate_Should_Fail_When_GitHubMissesSecret()
    {
        var options = new AuthenticationProvidersOptions
        {
            GitHub = new OAuthProviderOptions { Enabled = true, ClientId = "id" }
        };

        Validate(options).Should().ContainSingle();
    }

    [Fact]
    public void Registry_Should_RedirectToOnlyProvider_When_AutoRedirect()
    {
        var registry = new ExternalProviderRegistry(Options.Create(new AuthenticationProvidersOptions
        {
            Mode = AuthenticationMode.External,
            AutoRedirect = true,
            GitHub = new OAuthProviderOptions { Enabled = true, ClientId = "id", ClientSecret = "secret" }
        }));

        registry.AutoRedirectScheme.Should().Be(ExternalProviderRegistry.GitHubScheme);
        registry.RedirectProviders.Should().ContainSingle(p => p.Type == "github");
    }

    [Fact]
    public void Registry_Should_NotRedirect_When_AutoRedirectOff()
    {
        var registry = new ExternalProviderRegistry(Options.Create(new AuthenticationProvidersOptions
        {
            Mode = AuthenticationMode.External,
            GitHub = new OAuthProviderOptions { Enabled = true, ClientId = "id", ClientSecret = "secret" }
        }));

        registry.AutoRedirectScheme.Should().BeNull();
    }
}
