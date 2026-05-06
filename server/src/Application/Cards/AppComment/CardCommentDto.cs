namespace Snapflow.Application.Cards.AddComment;

public sealed record CardCommentDto(
    int Id,
    int UserId,
    string UserName,
    string Content,
    DateTimeOffset CreatedAt);