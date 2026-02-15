namespace Snapflow.Application.Lists.GetByBoardId;

public sealed class GetListsByBoardId
{
    public sealed record ListDto(
        int Id,
        int BoardId,
        int SwimlaneId,
        string Title,
        string Rank,
        int? Width);
}

