using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;

namespace Snapflow.Api.Hubs.Board.ClientEventHandlers;

public sealed class SwimlaneUpdatedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext) : IDomainEventHandler<SwimlaneUpdatedDomainEvent>
{
    public Task Handle(SwimlaneUpdatedDomainEvent domainEvent, CancellationToken cancellationToken)
        => hubContext.Clients.Group(domainEvent.BoardId).SwimlaneUpdated(new(domainEvent.Id, domainEvent.Title,
            domainEvent.Height), cancellationToken);
}