using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Lists;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class ListMovedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    BoardHubUsers users) : IDomainEventHandler<ListMovedDomainEvent>
{
    public async Task Handle(ListMovedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        IBoardHubClient.UserDto? movedBy = await users.FindAsync(domainEvent.MovedById, cancellationToken);
        await hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.ListMoved(new(domainEvent.Id, domainEvent.SwimlaneId, domainEvent.Rank,
                showUsers ? movedBy : null), cancellationToken));
    }
}
