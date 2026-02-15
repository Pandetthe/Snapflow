using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Lists;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class ListDeletedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext) : IDomainEventHandler<ListDeletedDomainEvent>
{
    public Task Handle(ListDeletedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        hubContext.Clients
            .GroupExcept(domainEvent.BoardId, domainEvent.ConnectionId)
            .ListDeleted(new(domainEvent.Id), cancellationToken);
}