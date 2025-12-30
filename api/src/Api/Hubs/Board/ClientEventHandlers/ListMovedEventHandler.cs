using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Lists;

namespace Snapflow.Api.Hubs.Board.ClientEventHandlers;

public sealed class ListMovedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext) : IDomainEventHandler<ListMovedDomainEvent>
{
    public Task Handle(ListMovedDomainEvent domainEvent, CancellationToken cancellationToken)
        => hubContext.Clients.Group(domainEvent.BoardId).ListMoved(new(domainEvent.Id, domainEvent.SwimlaneId, domainEvent.Rank), cancellationToken);
}