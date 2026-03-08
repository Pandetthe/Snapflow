using System.Diagnostics.CodeAnalysis;
using static Snapflow.Application.Swimlanes.Create.CreateSwimlaneResponse;

namespace Snapflow.Application.Swimlanes.Create;

public sealed record CreateSwimlaneResponse(
    int Id,
    string Rank,
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