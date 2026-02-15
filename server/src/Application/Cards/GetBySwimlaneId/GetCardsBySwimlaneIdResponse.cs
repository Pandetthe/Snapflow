namespace Snapflow.Application.Cards.GetBySwimlaneId;

public sealed class GetCardsBySwimlaneIdResponse
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