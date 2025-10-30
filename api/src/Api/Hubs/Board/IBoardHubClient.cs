using Snapflow.Domain.Members;

namespace Snapflow.Api.Hubs.Board;

internal interface IBoardHubClient
{
    Task BoardUpdated();

    Task BoardDeleted(CancellationToken cancellationToken = default);

    Task SwimlaneCreated();

    Task SwimlaneUpdated();

    Task SwimlaneDeleted();

    Task ListCreated();

    Task ListUpdated();

    Task ListDeleted();

    Task TagCreated();

    Task TagUpdated();

    Task TagDeleted();

    Task CardLocked();

    Task CardUnlocked();

    Task CardCreated();

    Task CardUpdated();

    Task CardDeleted();

    Task CardMoved();

    Task MemberRemoved(int userId, CancellationToken cancellationToken = default);

    Task YourRoleChanged(MemberRole oldRole, MemberRole newRole, CancellationToken cancellationToken = default);
}