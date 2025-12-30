using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Lists;

namespace Snapflow.Api.Hubs.Board.ClientEventHandlers;

public sealed class ListUpdatedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext) : IDomainEventHandler<ListUpdatedDomainEvent>
{
    public Task Handle(ListUpdatedDomainEvent domainEvent, CancellationToken cancellationToken)
        => hubContext.Clients.Group(domainEvent.BoardId).ListUpdated(new(domainEvent.Id, domainEvent.Title, domainEvent.Width), cancellationToken);
}

