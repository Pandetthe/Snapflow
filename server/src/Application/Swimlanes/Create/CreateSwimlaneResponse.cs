using static Snapflow.Application.Swimlanes.Create.CreateSwimlaneResponse;

namespace Snapflow.Application.Swimlanes.Create;

public sealed record CreateSwimlaneResponse(
    int Id,
    string Rank,
    DateTimeOffset CreatedAt,
    UserDto CreatedBy)
{
    public sealed record UserDto(int Id, string UserName);
}