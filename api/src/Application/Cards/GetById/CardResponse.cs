namespace Snapflow.Application.Cards.GetById;

public sealed record CardResponse(
    int Id,
    int ListId,
    int SwimlaneId,
    int BoardId,
    string Title,
    string Description,
    DateTimeOffset CreatedAt,
    UserResponse CreatedBy,
    DateTimeOffset? UpdatedAt,
    UserResponse? UpdatedBy);

public sealed record UserResponse(
    int Id,
    string UserName);