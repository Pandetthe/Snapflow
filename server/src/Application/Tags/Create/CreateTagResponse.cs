using static Snapflow.Application.Tags.Create.CreateTagResponse;

namespace Snapflow.Application.Tags.Create;

public sealed record CreateTagResponse(
    int Id,
    DateTimeOffset CreatedAt,
    UserDto CreatedBy)
{
    public sealed record UserDto(int Id, string UserName);
}
