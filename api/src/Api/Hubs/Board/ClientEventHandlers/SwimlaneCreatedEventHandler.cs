using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;

namespace Snapflow.Api.Hubs.Board.ClientEventHandlers;

public sealed class SwimlaneCreatedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext) : IDomainEventHandler<SwimlaneCreatedDomainEvent>
{
    public Task Handle(SwimlaneCreatedDomainEvent domainEvent, CancellationToken cancellationToken) 
        => hubContext.Clients.Group(domainEvent.BoardId).SwimlaneCreated(new(domainEvent.Id, domainEvent.Title,
            domainEvent.Height, domainEvent.Rank), cancellationToken);
}
