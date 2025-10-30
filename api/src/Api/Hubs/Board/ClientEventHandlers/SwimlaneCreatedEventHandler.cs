using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;

namespace Snapflow.Api.Hubs.Board.ClientEventHandlers;

internal sealed class SwimlaneCreatedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext) : IDomainEventHandler<SwimlaneCreatedDomainEvent>
{
    public Task Handle(SwimlaneCreatedDomainEvent domainEvent, CancellationToken cancellationToken) 
        => hubContext.Clients.Group(domainEvent.BoardId).SwimlaneCreated(domainEvent.Id, domainEvent.Title, cancellationToken);
}
