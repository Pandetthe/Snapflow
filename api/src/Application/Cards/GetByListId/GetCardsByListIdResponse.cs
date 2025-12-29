namespace Snapflow.Application.Cards.GetByListId;

public sealed class GetCardsByListIdResponse
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