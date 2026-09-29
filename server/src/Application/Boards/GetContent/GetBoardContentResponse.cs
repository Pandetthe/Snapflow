using System.Diagnostics.CodeAnalysis;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Tags;
using static Snapflow.Application.Boards.GetContent.GetBoardContentResponse;

namespace Snapflow.Application.Boards.GetContent;

public sealed record GetBoardContentResponse(
        int Id,
        string Title,
        string Description,
        BoardVisibility Visibility,
        IReadOnlyList<SwimlaneDto> Swimlanes,
        IReadOnlyList<TagDto> Tags)
{
    public sealed record UserDto(int Id, string UserName)
    {
        // Filled in after the query, avatar URLs are not part of the database projection.
        public string? AvatarUrl { get; init; }

        [return: NotNullIfNotNull(nameof(user))]
        public static UserDto? From(Domain.Users.IUser? user) =>
            user == null ? null : new UserDto(user.Id, user.UserName);
    }


    // The board's tag definitions; cards carry only the ids.
    public sealed record TagDto(int Id, string Title, TagColors Color);

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
        string Description,
        string Rank,
        DateTimeOffset CreatedAt,
        UserDto? CreatedBy,
        DateTimeOffset? UpdatedAt,
        UserDto? UpdatedBy,
        IReadOnlyList<int> TagIds);
}