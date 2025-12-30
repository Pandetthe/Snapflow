using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Boards;

namespace Snapflow.Api.Hubs.Board.ClientEventHandlers;

public sealed class BoardUpdatedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext) : IDomainEventHandler<BoardUpdatedDomainEvent>
{
    public Task Handle(BoardUpdatedDomainEvent domainEvent, CancellationToken cancellationToken)
        => hubContext.Clients.Group(domainEvent.Id).BoardUpdated(new(domainEvent.Title, domainEvent.Description), cancellationToken);
}
