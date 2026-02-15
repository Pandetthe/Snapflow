namespace Snapflow.Application.Swimlanes.GetByBoardId;

public sealed class GetSwimlanesByBoardIdResponse
{
    public sealed record SwimlaneDto(int Id, string Title, string Rank, int? Height);
}