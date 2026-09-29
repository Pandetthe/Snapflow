using static Snapflow.Application.Tags.Update.UpdateTagResponse;

namespace Snapflow.Application.Tags.Update;

/// <summary>
/// What the edit stamped on the element. Both are null when the edit changed nothing, so nothing was stamped.
/// </summary>
public sealed record UpdateTagResponse(
    DateTimeOffset? UpdatedAt,
    UserDto? UpdatedBy)
{
    public sealed record UserDto(int Id, string UserName);
}
