using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Jdenticon;
using Jdenticon.Rendering;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Infrastructure.Common;

namespace Snapflow.Infrastructure.Services;

internal sealed class AvatarService(IOptions<ServicesOptions> options) : IAvatarService
{
    public string GenerateAvatarUrl(int userId) => $"{options.Value.ApiUrl}/users/{userId}/avatar";

    public string GetGravatarUrl(string email, int size = 200)
    {
        var hash = ComputeMd5Hash(email.Trim().ToLowerInvariant());
        return $"https://www.gravatar.com/avatar/{hash}?s={size}&d=mp";
    }

    public string GenerateJdenticonSvg(int userId, int size = 200)
    {
        return CreateIcon(userId, size).ToSvg();
    }

    private static Identicon CreateIcon(int userId, int size)
    {
        Identicon icon = Identicon.FromValue(userId.ToString(CultureInfo.InvariantCulture), size);
        icon.Style = new IdenticonStyle
        {
            BackColor = Color.Transparent,
            Padding = 0.08f 
        };
        return icon;
    }

    [SuppressMessage("Security", "CA5351:Do Not Use Broken Cryptographic Algorithms", 
        Justification = "Gravatar API requires MD5 for avatar identification.")]
    private static string ComputeMd5Hash(string input)
    {
        var hashBytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}