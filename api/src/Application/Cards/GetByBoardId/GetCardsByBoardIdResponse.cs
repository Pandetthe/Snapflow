namespace Snapflow.Application.Cards.GetByBoardId;

public sealed class GetCardsByBoardIdResponse
{
    public sealed record CardDto(
        int Id,
        int ListId,
        int SwimlaneId,
        int BoardId,
        string Title,
        string Description,
        string Rank);
}