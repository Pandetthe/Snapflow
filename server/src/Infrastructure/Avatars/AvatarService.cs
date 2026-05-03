using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Jdenticon;
using Jdenticon.Rendering;
using Microsoft.Extensions.Options;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Application.Users.Avatar;
using Snapflow.Common;
using Snapflow.Domain.Users;
using Snapflow.Infrastructure.Common;

namespace Snapflow.Infrastructure.Avatars;

internal sealed class AvatarService(
    IOptions<ServicesOptions> options,
    IHttpClientFactory httpClientFactory) : IAvatarService
{
    public string GenerateAvatarUrl(int userId) => $"{options.Value.ApiUrl}/users/{userId}/avatar";

    public async Task<Result<AvatarResponse>> GetAvatarDataAsync(IUser user, CancellationToken cancellationToken)
    {
        return user.AvatarType switch
        {
            AvatarType.Uploaded => user.AvatarData is { Length: > 0 }
                ? Result.Success(new AvatarResponse(user.AvatarData, user.AvatarContentType ?? "image/png"))
                : Result.Failure<AvatarResponse>(UserErrors.AvatarDataMissing),

            AvatarType.Generated => GetJdenticonResponse(user.Id),

            AvatarType.Gravatar => await FetchGravatarAsync(user, cancellationToken),

            _ => Result.Failure<AvatarResponse>(UserErrors.AvatarInvalidFileType)
        };
    }

    private static Result<AvatarResponse> GetJdenticonResponse(int userId) =>
        Result.Success(new AvatarResponse(
            Encoding.UTF8.GetBytes(CreateIcon(userId, 200).ToSvg()),
            "image/svg+xml"));

    private async Task<Result<AvatarResponse>> FetchGravatarAsync(IUser user, CancellationToken cancellationToken)
    {
        var hash = ComputeMd5Hash(user.Email.Trim().ToLowerInvariant());

        try
        {
            HttpClient client = httpClientFactory.CreateClient("gravatar");
            using HttpResponseMessage response = await client.GetAsync(
                $"/avatar/{hash}?s=200&d=404", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                var contentType = response.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
                return Result.Success(new AvatarResponse(data, contentType));
            }
        }
        catch (HttpRequestException) { }

        return GetJdenticonResponse(user.Id);
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
