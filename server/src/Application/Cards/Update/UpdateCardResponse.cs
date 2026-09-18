using System.Diagnostics.CodeAnalysis;
using static Snapflow.Application.Cards.Update.UpdateCardResponse;

namespace Snapflow.Application.Cards.Update;

/// <summary>
/// What the edit stamped on the element. Both are null when the edit changed nothing, so nothing was stamped.
/// </summary>
public sealed record UpdateCardResponse(
    DateTimeOffset? UpdatedAt,
    UserDto? UpdatedBy)
{
    public sealed record UserDto(int Id, string UserName)
    {
        [return: NotNullIfNotNull(nameof(user))]
        public static UserDto? From(Domain.Users.IUser? user) =>
            user == null ? null : new UserDto(user.Id, user.UserName);
    }
}