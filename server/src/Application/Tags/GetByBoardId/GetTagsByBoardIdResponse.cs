using Snapflow.Domain.Tags;

namespace Snapflow.Application.Tags.GetByBoardId;

public sealed class GetTagsByBoardIdResponse
{
    public sealed record TagDto(int Id, string Title, TagColors Color);
}
