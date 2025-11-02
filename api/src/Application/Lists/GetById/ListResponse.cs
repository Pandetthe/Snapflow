namespace Snapflow.Application.Lists.GetById;

public sealed record ListResponse(
    int Id,
    int BoardId,
    int SwimlaneId,
    string Title,
    DateTimeOffset CreatedAt,
    UserResponse CreatedBy,
    DateTimeOffset? UpdatedAt,
    UserResponse? UpdatedBy);

public sealed record UserResponse(
    int Id,
    string UserName);