using Snapflow.Domain.Members;

namespace Snapflow.Api.Hubs.Board;

internal interface IBoardHubClient
{
    Task BoardUpdated();

    Task BoardDeleted(CancellationToken cancellationToken = default);

    Task SwimlaneCreated(int id, string title, CancellationToken cancellationToken = default);

    Task SwimlaneUpdated();

    Task SwimlaneDeleted(int id, CancellationToken cancellationToken = default);

    Task ListCreated(int id, int swimlaneId, string title, CancellationToken cancellationToken = default);

    Task ListUpdated();

    Task ListDeleted(int id, CancellationToken cancellationToken = default);

    Task TagCreated();

    Task TagUpdated();

    Task TagDeleted();

    Task CardLocked();

    Task CardUnlocked();

    Task CardCreated(int id, int listId, int swimlaneId, string title, string description, CancellationToken cancellationToken = default);

    Task CardUpdated();

    Task CardDeleted(int id, CancellationToken cancellationToken = default);

    Task CardMoved();

    Task MemberRemoved(int userId, CancellationToken cancellationToken = default);

    Task YourRoleChanged(MemberRole oldRole, MemberRole newRole, CancellationToken cancellationToken = default);
}