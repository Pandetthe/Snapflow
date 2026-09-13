using static Snapflow.Application.Cards.Create.CreateCardResponse;

namespace Snapflow.Application.Cards.Create;

public sealed record CreateCardResponse(
    int Id,
    string Rank,
    DateTimeOffset CreatedAt,
    UserDto CreatedBy)
{
    public sealed record UserDto(int Id, string UserName, string AvatarUrl);
}