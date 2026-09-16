using Microsoft.AspNetCore.SignalR;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Boards;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class BoardVisibilityChangedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    BoardConnectionRegistry connectionRegistry,
    IBoardVisibilityPolicy visibilityPolicy) : IDomainEventHandler<BoardVisibilityChangedDomainEvent>
{
    public async Task Handle(BoardVisibilityChangedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        foreach (var (connectionId, isAuthenticated) in connectionRegistry.GetGuests(domainEvent.Id))
        {
            if (visibilityPolicy.CanNonMemberView(domainEvent.NewVisibility, isAuthenticated)
                || !connectionRegistry.RemoveGuest(domainEvent.Id, connectionId))
                continue;

            await hubContext.Groups.RemoveFromGroupAsync(
                connectionId, BoardHubExtensions.GuestsGroupName(domainEvent.Id), cancellationToken);
            await hubContext.Clients.Client(connectionId).RemovedFromBoard(cancellationToken);
        }

        await hubContext.Clients.SendToBoard(domainEvent.Id, domainEvent.ConnectionId, (clients, _) =>
            clients.BoardVisibilityChanged(new(domainEvent.NewVisibility), cancellationToken));
    }
}
