using System.Diagnostics.CodeAnalysis;
using static Snapflow.Application.Cards.Update.UpdateCardResponse;

namespace Snapflow.Application.Cards.Update;

public sealed record UpdateCardResponse(
    DateTimeOffset UpdatedAt,
    UserDto UpdatedBy)
{
    public sealed record UserDto(int Id, string UserName)
    {
        [return: NotNullIfNotNull(nameof(user))]
        public static UserDto? From(Domain.Users.IUser? user) =>
            user == null ? null : new UserDto(user.Id, user.UserName);
    }
}