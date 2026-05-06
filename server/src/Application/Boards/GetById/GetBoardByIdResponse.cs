using System.Diagnostics.CodeAnalysis;
using Snapflow.Application.Cards.AddComment;
using static Snapflow.Application.Boards.GetById.GetBoardByIdResponse;

namespace Snapflow.Application.Boards.GetById;

public sealed record GetBoardByIdResponse(
        int Id,
        string Title,
        string? Description,
        IReadOnlyList<SwimlaneDto> Swimlanes)
{
    public sealed record UserDto(int Id, string UserName)
    {
        [return: NotNullIfNotNull(nameof(user))]
        public static UserDto? From(Domain.Users.IUser? user) =>
            user == null ? null : new UserDto(user.Id, user.UserName);
    }


    public sealed record SwimlaneDto(
        int Id,
        string Title,
        string Rank,
        int? Height,
        IReadOnlyList<ListDto> Lists);

    public sealed record ListDto(
        int Id,
        string Title,
        string Rank,
        int? Width,
        IReadOnlyList<CardDto> Cards);

    public sealed record CardDto(
        int Id,
        string Title,
        string? Description,
        string Rank,
        DateTimeOffset CreatedAt,
        UserDto CreatedBy,
        DateTimeOffset? UpdatedAt,
        UserDto? UpdatedBy,
        List<CardCommentDto> Comments);
    public sealed record CardCommentDto(
        int Id,
        int UserId,
        string UserName,
        string Content,
        DateTimeOffset CreatedAt);
}