using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;

namespace Snapflow.Api.Hubs.Board.ClientEventHandlers;

public sealed class SwimlaneMovedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext) : IDomainEventHandler<SwimlaneMovedDomainEvent>
{
    public Task Handle(SwimlaneMovedDomainEvent domainEvent, CancellationToken cancellationToken)
        => hubContext.Clients.Group(domainEvent.BoardId).SwimlaneMoved(new(domainEvent.Id, domainEvent.Rank), cancellationToken);
}