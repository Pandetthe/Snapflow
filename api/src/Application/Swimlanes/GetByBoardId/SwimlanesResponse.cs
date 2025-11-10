using System.Collections.ObjectModel;

namespace Snapflow.Application.Swimlanes.GetByBoardId;

public sealed class SwimlanesResponse : ReadOnlyCollection<SwimlaneResponse>
{
    public SwimlanesResponse(IEnumerable<SwimlaneResponse> enumerable) : base(enumerable.ToList())
    {
    }
}

public sealed record SwimlaneResponse(int Id, string Title, string Rank);