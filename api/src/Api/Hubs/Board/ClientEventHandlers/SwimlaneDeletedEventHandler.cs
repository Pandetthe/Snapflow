using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;

namespace Snapflow.Api.Hubs.Board.ClientEventHandlers;

public sealed class SwimlaneDeletedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext) : IDomainEventHandler<SwimlaneDeletedDomainEvent>
{
    public Task Handle(SwimlaneDeletedDomainEvent domainEvent, CancellationToken cancellationToken)
        => hubContext.Clients.Group(domainEvent.BoardId).SwimlaneDeleted(new(domainEvent.Id), cancellationToken);
}
