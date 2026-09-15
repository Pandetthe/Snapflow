using Snapflow.Application.Boards.GetById;
using Snapflow.Application.Boards.GetDetails;
using Snapflow.Domain.Members;
using Snapflow.Domain.Tags;

namespace Snapflow.Presentation.Hubs.Board;

public interface IBoardHubClient
{
    // The whole board, sent to a connection when it connects or reconnects; it replaces the client's state.
    public sealed record BoardSnapshotPayload(int Id, string Title, string Description,
        IReadOnlyList<GetBoardByIdResponse.SwimlaneDto> Swimlanes, IReadOnlyList<GetBoardByIdResponse.TagDto> Tags,
        IReadOnlyList<GetBoardDetailsMemberResponse> Members);

    Task BoardSnapshot(BoardSnapshotPayload payload, CancellationToken cancellationToken = default);

    public sealed record BoardUpdatedPayload(string Title, string Description);

    Task BoardUpdated(BoardUpdatedPayload payload, CancellationToken cancellationToken = default);

    Task BoardDeleted(CancellationToken cancellationToken = default);

    public sealed record SwimlaneCreatedPayload(int Id, string Title, int? Height, string Rank, UserDto CreatedBy);

    Task SwimlaneCreated(SwimlaneCreatedPayload payload, CancellationToken cancellationToken = default);

    public sealed record SwimlaneUpdatedPayload(int Id, string Title, int? Height, UserDto UpdatedBy);

    Task SwimlaneUpdated(SwimlaneUpdatedPayload payload, CancellationToken cancellationToken = default);

    public sealed record SwimlaneMovedPayload(int Id, string Rank, UserDto MovedBy);

    Task SwimlaneMoved(SwimlaneMovedPayload payload, CancellationToken cancellationToken = default);

    public sealed record SwimlaneDeletedPayload(int Id, UserDto DeletedBy);

    Task SwimlaneDeleted(SwimlaneDeletedPayload payload, CancellationToken cancellationToken = default);

    public sealed record ListCreatedPayload(int Id, int SwimlaneId, string Title, int? Width, string Rank, UserDto CreatedBy);

    Task ListCreated(ListCreatedPayload payload, CancellationToken cancellationToken = default);

    public sealed record ListUpdatedPayload(int Id, string Title, int? Width, UserDto UpdatedBy);

    Task ListUpdated(ListUpdatedPayload payload, CancellationToken cancellationToken = default);

    public sealed record ListMovedPayload(int Id, int SwimlaneId, string Rank, UserDto MovedBy);

    Task ListMoved(ListMovedPayload payload, CancellationToken cancellationToken = default);

    public sealed record ListDeletedPayload(int Id, UserDto DeletedBy);

    Task ListDeleted(ListDeletedPayload payload, CancellationToken cancellationToken = default);

    Task CardLocked();

    Task CardUnlocked();

    public sealed record UserDto(int Id, string UserName, string AvatarUrl);

    public sealed record CardCreatedPayload(int Id, int ListId, string Title, string Description, string Rank,
        DateTimeOffset CreatedAt, UserDto CreatedBy);

    Task CardCreated(CardCreatedPayload payload, CancellationToken cancellationToken = default);

    public sealed record CardUpdatedPayload(int Id, string Title, string Description, UserDto UpdatedBy);

    Task CardUpdated(CardUpdatedPayload payload, CancellationToken cancellationToken = default);

    public sealed record CardDeletedPayload(int Id, UserDto DeletedBy);

    Task CardDeleted(CardDeletedPayload payload, CancellationToken cancellationToken = default);

    public sealed record CardMovedPayload(int Id, int ListId, string Rank, UserDto MovedBy);

    Task CardMoved(CardMovedPayload payload, CancellationToken cancellationToken = default);

    public sealed record TagCreatedPayload(int Id, string Title, TagColors Color, UserDto CreatedBy);

    Task TagCreated(TagCreatedPayload payload, CancellationToken cancellationToken = default);

    public sealed record TagUpdatedPayload(int Id, string Title, TagColors Color, UserDto UpdatedBy);

    Task TagUpdated(TagUpdatedPayload payload, CancellationToken cancellationToken = default);

    public sealed record TagDeletedPayload(int Id, UserDto DeletedBy);

    Task TagDeleted(TagDeletedPayload payload, CancellationToken cancellationToken = default);

    public sealed record CardTagAddedPayload(int CardId, int TagId, UserDto AddedBy);

    Task CardTagAdded(CardTagAddedPayload payload, CancellationToken cancellationToken = default);

    public sealed record CardTagRemovedPayload(int CardId, int TagId, UserDto RemovedBy);

    Task CardTagRemoved(CardTagRemovedPayload payload, CancellationToken cancellationToken = default);

    Task MemberRemoved(int userId, CancellationToken cancellationToken = default);

    Task YourRoleChanged(MemberRole oldRole, MemberRole newRole, CancellationToken cancellationToken = default);
}