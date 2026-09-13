using System.Diagnostics.CodeAnalysis;
using static Snapflow.Application.Boards.GetById.GetBoardByIdResponse;

namespace Snapflow.Application.Boards.GetById;

public sealed record GetBoardByIdResponse(
        int Id,
        string Title,
        string Description,
        IReadOnlyList<SwimlaneDto> Swimlanes)
{
    public sealed record UserDto(int Id, string UserName)
    {
        // Filled in after the query, avatar URLs are not part of the database projection.
        public string? AvatarUrl { get; set; }

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
        string Description,
        string Rank,
        DateTimeOffset CreatedAt,
        UserDto CreatedBy,
        DateTimeOffset? UpdatedAt,
        UserDto? UpdatedBy);
}