using System.Diagnostics.CodeAnalysis;
using static Snapflow.Application.Swimlanes.Update.UpdateSwimlaneResponse;

namespace Snapflow.Application.Swimlanes.Update;

public sealed record UpdateSwimlaneResponse(
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