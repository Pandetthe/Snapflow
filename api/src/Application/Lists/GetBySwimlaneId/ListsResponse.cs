using System.Collections.ObjectModel;

namespace Snapflow.Application.Lists.GetBySwimlaneId;

public sealed class ListsResponse : ReadOnlyCollection<ListResponse>
{
    public ListsResponse(IEnumerable<ListResponse> enumerable) : base(enumerable.ToList())
    {
    }
}

public sealed record ListResponse(
    int Id,
    int BoardId,
    int SwimlaneId,
    string Title);