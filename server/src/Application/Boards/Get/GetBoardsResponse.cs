using System.Diagnostics.CodeAnalysis;

namespace Snapflow.Application.Boards.Get;

public sealed class GetBoardsResponse
{
    public sealed record UserDto(int Id, string UserName)
    {
        [return: NotNullIfNotNull(nameof(user))]
        public static UserDto? From(Domain.Users.IUser? user) =>
            user == null ? null : new UserDto(user.Id, user.UserName);
    }

    public sealed record BoardDto(
        int Id,
        string Title,
        string Description,
        DateTimeOffset CreatedAt,
        UserDto CreatedBy,
        DateTimeOffset? UpdatedAt,
        UserDto? UpdatedBy);
}