using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Boards;

namespace Snapflow.Api.Hubs.Board.ClientEventHandlers;

internal sealed class BoardDeletedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext) : IDomainEventHandler<BoardDeletedDomainEvent>
{
    public Task Handle(BoardDeletedDomainEvent domainEvent, CancellationToken cancellationToken)
        => hubContext.Clients.Group(domainEvent.Id).BoardDeleted(cancellationToken);
}
