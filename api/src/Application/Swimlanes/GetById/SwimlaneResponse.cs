namespace Snapflow.Application.Swimlanes.GetById;

public sealed record SwimlaneResponse(
    int Id,
    int BoardId,
    string Title,
    string Rank,
    DateTimeOffset CreatedAt,
    UserResponse CreatedBy,
    DateTimeOffset? UpdatedAt,
    UserResponse? UpdatedBy);

public sealed record UserResponse(int Id, string UserName);