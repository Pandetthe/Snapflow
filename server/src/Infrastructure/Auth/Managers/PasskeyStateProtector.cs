using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Snapflow.Infrastructure.Auth.Managers;

internal sealed record PasskeyCeremony(string Purpose, string? UserId, string? State);

internal sealed class PasskeyStateProtector(
    IDataProtectionProvider dataProtectionProvider,
    IDistributedCache cache)
{
    public const string Registration = "registration";
    public const string SignIn = "sign-in";
    public const string TwoFactor = "two-factor";

    private const string UsedStateKeyPrefix = "passkey-state:";

    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(10);

    private readonly ITimeLimitedDataProtector _protector = dataProtectionProvider
        .CreateProtector("Snapflow.Auth.Passkey")
        .ToTimeLimitedDataProtector();

    public string Protect(string purpose, string? userId, string? state) =>
        _protector.Protect(JsonSerializer.Serialize(new PasskeyCeremony(purpose, userId, state)), Lifetime);

    public async Task<PasskeyCeremony?> ConsumeAsync(string protectedState, string purpose)
    {
        PasskeyCeremony? ceremony = Unprotect(protectedState);
        if (ceremony is null || !string.Equals(ceremony.Purpose, purpose, StringComparison.Ordinal))
            return null;

        string key = UsedStateKeyPrefix + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(protectedState)));
        if (await cache.GetAsync(key) is not null)
            return null;

        await cache.SetAsync(key, [1], new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = Lifetime });
        return ceremony;
    }

    private PasskeyCeremony? Unprotect(string protectedState)
    {
        try
        {
            return JsonSerializer.Deserialize<PasskeyCeremony>(_protector.Unprotect(protectedState));
        }
        catch (Exception exception) when (exception is CryptographicException or JsonException or FormatException)
        {
            return null;
        }
    }
}
