using System.Diagnostics.CodeAnalysis;
using static Snapflow.Application.Swimlanes.GetById.GetSwimlaneByIdResponse;

namespace Snapflow.Application.Swimlanes.GetById;

public sealed record GetSwimlaneByIdResponse(
    int Id,
    int BoardId,
    string Title,
    string Rank,
    int? Height,
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