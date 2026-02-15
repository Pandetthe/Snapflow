using Snapflow.Domain.Members;

namespace Snapflow.Presentation.Hubs.Board;

public interface IBoardHubClient
{
    public sealed record BoardUpdatedPayload(string Title, string Description);

    Task BoardUpdated(BoardUpdatedPayload payload, CancellationToken cancellationToken = default);

    Task BoardDeleted(CancellationToken cancellationToken = default);

    public sealed record SwimlaneCreatedPayload(int Id, string Title, int? Height, string Rank);

    Task SwimlaneCreated(SwimlaneCreatedPayload payload, CancellationToken cancellationToken = default);

    public sealed record SwimlaneUpdatedPayload(int Id, string Title, int? Height);

    Task SwimlaneUpdated(SwimlaneUpdatedPayload payload, CancellationToken cancellationToken = default);

    public sealed record SwimlaneMovedPayload(int Id, string Rank);

    Task SwimlaneMoved(SwimlaneMovedPayload payload, CancellationToken cancellationToken = default);

    public sealed record SwimlaneDeletedPayload(int Id);

    Task SwimlaneDeleted(SwimlaneDeletedPayload payload, CancellationToken cancellationToken = default);

    public sealed record ListCreatedPayload(int Id, int SwimlaneId, string Title, int? Width, string Rank);

    Task ListCreated(ListCreatedPayload payload, CancellationToken cancellationToken = default);

    public sealed record ListUpdatedPayload(int Id, string Title, int? Width);

    Task ListUpdated(ListUpdatedPayload payload, CancellationToken cancellationToken = default);

    public sealed record ListMovedPayload(int Id, int SwimlaneId, string Rank);

    Task ListMoved(ListMovedPayload payload, CancellationToken cancellationToken = default);

    public sealed record ListDeletedPayload(int Id);

    Task ListDeleted(ListDeletedPayload payload, CancellationToken cancellationToken = default);

    Task CardLocked();

    Task CardUnlocked();

    public sealed record CardCreatedPayload(int Id, int ListId, string Title, string Description, string Rank);

    Task CardCreated(CardCreatedPayload payload, CancellationToken cancellationToken = default);

    public sealed record CardUpdatedPayload(int Id, string Title, string Description);

    Task CardUpdated(CardUpdatedPayload payload, CancellationToken cancellationToken = default);

    public sealed record CardDeletedPayload(int Id);

    Task CardDeleted(CardDeletedPayload payload, CancellationToken cancellationToken = default);

    public sealed record CardMovedPayload(int Id, int ListId, string Rank);

    Task CardMoved(CardMovedPayload payload, CancellationToken cancellationToken = default);

    Task TagCreated();

    Task TagUpdated();

    Task TagDeleted();

    Task MemberRemoved(int userId, CancellationToken cancellationToken = default);

    Task YourRoleChanged(MemberRole oldRole, MemberRole newRole, CancellationToken cancellationToken = default);
}