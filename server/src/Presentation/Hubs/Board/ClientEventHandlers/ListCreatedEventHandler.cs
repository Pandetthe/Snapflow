using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Lists;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class ListCreatedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext) : IDomainEventHandler<ListCreatedDomainEvent>
{
    public Task Handle(ListCreatedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        hubContext.Clients
            .GroupExcept(domainEvent.BoardId, domainEvent.ConnectionId)
            .ListCreated(new(domainEvent.Id, domainEvent.SwimlaneId, domainEvent.Title,
                domainEvent.Width, domainEvent.Rank), cancellationToken);
}
