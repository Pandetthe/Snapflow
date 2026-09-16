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
        // Taken out of the board's group, so a client that stays connected stops being sent the
        // board. Every hub method checks the caller's permissions on each invocation, so a removed
        // member can no longer change anything either.
        List<string> connectionIds = connectionRegistry
            .GetConnectionIds(domainEvent.BoardId, domainEvent.UserId)
            .Where(connectionId => connectionRegistry.TryRemoveConnection(domainEvent.BoardId, domainEvent.UserId, connectionId))
            .ToList();

        await hubContext.Clients.Clients(connectionIds).RemovedFromBoard(cancellationToken);

        foreach (var connectionId in connectionIds)
        {
            await hubContext.Groups.RemoveFromGroupAsync(connectionId, $"{domainEvent.BoardId}", cancellationToken);
            await hubContext.Groups.RemoveFromGroupAsync(
                connectionId, $"{domainEvent.BoardId}-{domainEvent.UserId}", cancellationToken);
        }

        // The rest of the board updates its member list and stops showing them as a viewer.
        await hubContext.Clients
            .GroupExcept(domainEvent.BoardId, domainEvent.ConnectionId)
            .MemberRemoved(domainEvent.UserId, cancellationToken);

        if (connectionIds.Count > 0)
        {
            await hubContext.Clients
                .GroupExcept(domainEvent.BoardId, domainEvent.ConnectionId)
                .ViewerLeft(domainEvent.UserId, cancellationToken);
        }
    }
}
