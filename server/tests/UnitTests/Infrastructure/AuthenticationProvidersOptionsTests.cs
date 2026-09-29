using System.ComponentModel.DataAnnotations;
using AspNet.Security.OAuth.Apple;
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
        Assert.Empty(Validate(new AuthenticationProvidersOptions()));
    }

    [Fact]
    public void Validate_Should_Fail_When_ExternalModeWithoutProviders()
    {
        var options = new AuthenticationProvidersOptions { Mode = AuthenticationMode.External };

        Assert.Single(Validate(options));
    }

    [Fact]
    public void Validate_Should_Fail_When_EnabledProviderMissesSecret()
    {
        var options = new AuthenticationProvidersOptions
        {
            Google = new OAuthProviderOptions { Enabled = true, ClientId = "id" }
        };

        Assert.Single(Validate(options));
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

        Assert.Single(Validate(options));
    }

    [Fact]
    public void Validate_Should_Fail_When_LdapFilterHasNoPlaceholder()
    {
        var options = new AuthenticationProvidersOptions
        {
            Ldap = new LdapProviderOptions { Enabled = true, Host = "ldap", SearchBase = "dc=example,dc=com", UserFilter = "(uid=jan)" }
        };

        Assert.Single(Validate(options));
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

        Assert.Empty(registry.RedirectProviders);
        Assert.False(registry.LdapEnabled);
        Assert.True(registry.PasswordAuthenticationEnabled);
    }

    [Fact]
    public void Registry_Should_DisablePasswords_When_ExternalMode()
    {
        var registry = new ExternalProviderRegistry(Options.Create(new AuthenticationProvidersOptions
        {
            Mode = AuthenticationMode.External,
            OpenIdConnect = [new OpenIdConnectProviderOptions { Scheme = "corp", DisplayName = "Corp", Authority = "https://id.example.com", ClientId = "id" }]
        }));

        Assert.False(registry.PasswordAuthenticationEnabled);
        Assert.NotNull(registry.FindRedirectProvider("corp"));
        Assert.Null(registry.FindRedirectProvider("Google"));
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

        Assert.Single(Validate(options));
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

        Assert.Single(Validate(options));
    }

    [Fact]
    public void Validate_Should_Fail_When_GitHubMissesSecret()
    {
        var options = new AuthenticationProvidersOptions
        {
            GitHub = new OAuthProviderOptions { Enabled = true, ClientId = "id" }
        };

        Assert.Single(Validate(options));
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

        Assert.Equal(ExternalProviderRegistry.GitHubScheme, registry.AutoRedirectScheme);
        Assert.Single(registry.RedirectProviders, p => p.Type == "github");
    }

    [Fact]
    public void Validate_Should_Fail_When_AppleMissesSigningDetails()
    {
        var options = new AuthenticationProvidersOptions
        {
            Apple = new AppleProviderOptions { Enabled = true, ClientId = "pl.snapflow.web", PrivateKeyPath = "/keys/AuthKey.p8" }
        };

        Assert.Single(Validate(options));
    }

    [Fact]
    public void Validate_Should_Fail_When_AppleMissesPrivateKeyPath()
    {
        var options = new AuthenticationProvidersOptions
        {
            Apple = new AppleProviderOptions { Enabled = true, ClientId = "pl.snapflow.web", TeamId = "TEAM123456", KeyId = "KEY1234567" }
        };

        Assert.Single(Validate(options));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(181)]
    public void Validate_Should_Fail_When_AppleClientSecretExpiryOutOfRange(int days)
    {
        var options = new AuthenticationProvidersOptions
        {
            Apple = new AppleProviderOptions
            {
                Enabled = true,
                ClientId = "pl.snapflow.web",
                TeamId = "TEAM123456",
                KeyId = "KEY1234567",
                PrivateKeyPath = "/keys/AuthKey.p8",
                ClientSecretExpiryDays = days
            }
        };

        Assert.Single(Validate(options));
    }

    [Fact]
    public void Validate_Should_Pass_When_AppleFullyConfigured()
    {
        var options = new AuthenticationProvidersOptions
        {
            Apple = new AppleProviderOptions
            {
                Enabled = true,
                ClientId = "pl.snapflow.web",
                TeamId = "TEAM123456",
                KeyId = "KEY1234567",
                PrivateKeyPath = "/keys/AuthKey.p8"
            }
        };

        Assert.Empty(Validate(options));
    }

    [Fact]
    public void Registry_Should_OfferApple_When_Enabled()
    {
        var registry = new ExternalProviderRegistry(Options.Create(new AuthenticationProvidersOptions
        {
            Mode = AuthenticationMode.Mixed,
            Apple = new AppleProviderOptions
            {
                Enabled = true,
                ClientId = "pl.snapflow.web",
                TeamId = "TEAM123456",
                KeyId = "KEY1234567",
                PrivateKeyPath = "/keys/AuthKey.p8"
            }
        }));

        ExternalProvider? apple = registry.FindRedirectProvider(AppleAuthenticationDefaults.AuthenticationScheme);

        Assert.NotNull(apple);
        Assert.Equal("apple", apple!.Type);
        Assert.Equal("Apple", apple.DisplayName);
        Assert.True(apple.TrustEmail);
    }

    [Fact]
    public void Registry_Should_NotRedirect_When_AutoRedirectOff()
    {
        var registry = new ExternalProviderRegistry(Options.Create(new AuthenticationProvidersOptions
        {
            Mode = AuthenticationMode.External,
            GitHub = new OAuthProviderOptions { Enabled = true, ClientId = "id", ClientSecret = "secret" }
        }));

        Assert.Null(registry.AutoRedirectScheme);
    }
}
