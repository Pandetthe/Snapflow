namespace Snapflow.Application.Lists.GetBySwimlaneId;

public sealed class GetListsBySwimlaneIdResponse
{
    public sealed record ListDto(
        int Id,
        int BoardId,
        int SwimlaneId,
        string Title,
        string Rank,
        int? Width);
}

