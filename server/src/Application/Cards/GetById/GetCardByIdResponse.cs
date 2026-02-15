using System.Diagnostics.CodeAnalysis;
using static Snapflow.Application.Cards.GetById.GetCardByIdResponse;

namespace Snapflow.Application.Cards.GetById;

public sealed record GetCardByIdResponse(
    int Id,
    int ListId,
    int SwimlaneId,
    int BoardId,
    string Title,
    string Description,
    string Rank,
    DateTimeOffset CreatedAt,
    UserDto CreatedBy,
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