using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Lists;

namespace Snapflow.Api.Hubs.Board.ClientEventHandlers;

internal sealed class ListDeletedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext) : IDomainEventHandler<ListDeletedDomainEvent>
{
    public Task Handle(ListDeletedDomainEvent domainEvent, CancellationToken cancellationToken)
        => hubContext.Clients.Group(domainEvent.BoardId).ListDeleted(
            domainEvent.Id, cancellationToken);
}