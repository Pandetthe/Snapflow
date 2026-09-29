using static Snapflow.Application.Lists.Create.CreateListResponse;

namespace Snapflow.Application.Lists.Create;

public sealed record CreateListResponse(
    int Id,
    string Rank,
    DateTimeOffset CreatedAt,
    UserDto CreatedBy)
{
    public sealed record UserDto(int Id, string UserName);
}