using System.Diagnostics.CodeAnalysis;
using static Snapflow.Application.Swimlanes.Update.UpdateSwimlaneResponse;

namespace Snapflow.Application.Swimlanes.Update;

/// <summary>
/// What the edit stamped on the element. Both are null when the edit changed nothing, so nothing was stamped.
/// </summary>
public sealed record UpdateSwimlaneResponse(
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