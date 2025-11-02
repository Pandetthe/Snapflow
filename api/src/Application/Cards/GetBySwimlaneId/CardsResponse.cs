using System.Collections.ObjectModel;

namespace Snapflow.Application.Cards.GetBySwimlaneId;

public sealed class CardsResponse : ReadOnlyCollection<CardResponse>
{
    public CardsResponse(IEnumerable<CardResponse> enumerable) : base(enumerable.ToList())
    {
    }
}

public sealed record CardResponse(
    int Id,
    int ListId,
    int SwimlaneId,
    int BoardId,
    string Title,
    string Description);