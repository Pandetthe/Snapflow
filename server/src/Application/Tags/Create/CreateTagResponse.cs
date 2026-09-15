using System.Diagnostics.CodeAnalysis;
using static Snapflow.Application.Tags.Create.CreateTagResponse;

namespace Snapflow.Application.Tags.Create;

public sealed record CreateTagResponse(
    int Id,
    DateTimeOffset CreatedAt,
    UserDto CreatedBy)
{
    public sealed record UserDto(int Id, string UserName)
    {
        [return: NotNullIfNotNull(nameof(user))]
        public static UserDto? From(Domain.Users.IUser? user) =>
            user == null ? null : new UserDto(user.Id, user.UserName);
    }
}
