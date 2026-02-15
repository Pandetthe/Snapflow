using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class SwimlaneMovedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext) : IDomainEventHandler<SwimlaneMovedDomainEvent>
{
    public Task Handle(SwimlaneMovedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        hubContext.Clients
            .GroupExcept(domainEvent.BoardId, domainEvent.ConnectionId)
            .SwimlaneMoved(new(domainEvent.Id, domainEvent.Rank), cancellationToken);
}