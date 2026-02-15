using System.Diagnostics.CodeAnalysis;
using static Snapflow.Application.Lists.GetById.GetListByIdResponse;

namespace Snapflow.Application.Lists.GetById;

public sealed record GetListByIdResponse(
    int Id,
    int BoardId,
    int SwimlaneId,
    string Title,
    string Rank,
    int? Width,
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