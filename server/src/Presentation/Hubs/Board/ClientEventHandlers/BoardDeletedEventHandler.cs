using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Boards;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class BoardDeletedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext) : IDomainEventHandler<BoardDeletedDomainEvent>
{
    public Task Handle(BoardDeletedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        hubContext.Clients.SendToBoard(domainEvent.Id, domainEvent.ConnectionId, (clients, _) =>
            clients.BoardDeleted(cancellationToken));
}
