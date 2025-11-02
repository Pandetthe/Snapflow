using System.Collections.ObjectModel;

namespace Snapflow.Application.Boards.GetById;

public sealed record BoardResponse(
    int Id,
    string Title,
    string Description,
    SwimlanesResponse Swimlanes,
    DateTimeOffset CreatedAt,
    UserResponse CreatedBy,
    DateTimeOffset? UpdatedAt,
    UserResponse? UpdatedBy);

public sealed record UserResponse(int Id, string UserName);

public sealed class SwimlanesResponse : ReadOnlyCollection<SwimlaneResponse>
{
    public SwimlanesResponse(IEnumerable<SwimlaneResponse> enumerable) : base(enumerable.ToList())
    {
    }
}

public sealed record SwimlaneResponse(
    int Id,
    string Title,
    ListsResponse Lists,
    DateTimeOffset CreatedAt,
    UserResponse CreatedBy,
    DateTimeOffset? UpdatedAt,
    UserResponse? UpdatedBy);

public sealed class ListsResponse : ReadOnlyCollection<ListResponse>
{
    public ListsResponse(IEnumerable<ListResponse> enumerable) : base(enumerable.ToList())
    {
    }
}

public sealed record ListResponse(
    int Id,
    string Title,
    DateTimeOffset CreatedAt,
    UserResponse CreatedBy,
    DateTimeOffset? UpdatedAt,
    UserResponse? UpdatedBy);
