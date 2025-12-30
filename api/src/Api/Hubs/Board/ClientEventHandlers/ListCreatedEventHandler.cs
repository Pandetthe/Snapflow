using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Lists;

namespace Snapflow.Api.Hubs.Board.ClientEventHandlers;

public sealed class ListCreatedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext) : IDomainEventHandler<ListCreatedDomainEvent>
{
    public Task Handle(ListCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
        => hubContext.Clients.Group(domainEvent.BoardId).ListCreated(new(domainEvent.Id, domainEvent.SwimlaneId, domainEvent.Title,
            domainEvent.Width, domainEvent.Rank), cancellationToken);
}
