using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class SwimlaneMovedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    BoardHubUsers users) : IDomainEventHandler<SwimlaneMovedDomainEvent>
{
    public async Task Handle(SwimlaneMovedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        IBoardHubClient.UserDto? movedBy = await users.FindAsync(domainEvent.MovedById, cancellationToken);
        await hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.SwimlaneMoved(new(domainEvent.Id, domainEvent.Rank,
                showUsers ? movedBy : null), cancellationToken));
    }
}
