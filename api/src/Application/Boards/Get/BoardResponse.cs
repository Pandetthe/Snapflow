namespace Snapflow.Application.Boards.Get;

public sealed record BoardResponse(
    int Id,
    string Title,
    string Description,
    DateTimeOffset CreatedAt,
    User CreatedBy,
    DateTimeOffset? UpdatedAt,
    User? UpdatedBy);

public sealed record User(int Id, string UserName);