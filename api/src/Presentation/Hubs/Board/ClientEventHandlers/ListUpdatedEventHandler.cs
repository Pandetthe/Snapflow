using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Lists;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class ListUpdatedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext) : IDomainEventHandler<ListUpdatedDomainEvent>
{
    public Task Handle(ListUpdatedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        hubContext.Clients
            .GroupExcept(domainEvent.BoardId, domainEvent.ConnectionId)
            .ListUpdated(new(domainEvent.Id, domainEvent.Title, domainEvent.Width), cancellationToken);
}

