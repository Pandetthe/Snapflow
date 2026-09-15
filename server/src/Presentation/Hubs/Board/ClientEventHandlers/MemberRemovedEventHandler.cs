using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Members;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class MemberRemovedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    BoardConnectionRegistry connectionRegistry) : IDomainEventHandler<MemberRemovedDomainEvent>
{
    public async Task Handle(MemberRemovedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        // Told first, while the connections are still in the board's groups, so the client can leave
        // of its own accord instead of going quiet.
        await hubContext.Clients
            .Group(domainEvent.BoardId, domainEvent.UserId)
            .RemovedFromBoard(cancellationToken);

        // Taken out of the board's group, so a client that stays connected stops being sent the
        // board. Every hub method checks the caller's permissions on each invocation, so a removed
        // member can no longer change anything either.
        IReadOnlyList<string> connectionIds =
            connectionRegistry.GetConnectionIds(domainEvent.BoardId, domainEvent.UserId);

        foreach (var connectionId in connectionIds)
        {
            await hubContext.Groups.RemoveFromGroupAsync(connectionId, $"{domainEvent.BoardId}", cancellationToken);
            await hubContext.Groups.RemoveFromGroupAsync(
                connectionId, $"{domainEvent.BoardId}-{domainEvent.UserId}", cancellationToken);
            connectionRegistry.Remove(domainEvent.BoardId, domainEvent.UserId, connectionId);
        }

        // The rest of the board updates its member list.
        await hubContext.Clients
            .GroupExcept(domainEvent.BoardId, domainEvent.ConnectionId)
            .MemberRemoved(domainEvent.UserId, cancellationToken);
    }
}
