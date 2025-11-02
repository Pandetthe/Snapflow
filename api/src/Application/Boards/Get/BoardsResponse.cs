using System.Collections.ObjectModel;

namespace Snapflow.Application.Boards.Get;

public sealed class BoardsResponse : ReadOnlyCollection<BoardResponse>
{
    public BoardsResponse(IEnumerable<BoardResponse> enumerable) : base(enumerable.ToList())
    {
    }
}

public sealed record BoardResponse(
    int Id,
    string Title,
    string Description,
    DateTimeOffset CreatedAt,
    UserResponse CreatedBy,
    DateTimeOffset? UpdatedAt,
    UserResponse? UpdatedBy);

public sealed record UserResponse(int Id, string UserName);