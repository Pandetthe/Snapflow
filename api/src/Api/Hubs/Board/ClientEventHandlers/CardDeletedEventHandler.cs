using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Cards;

namespace Snapflow.Api.Hubs.Board.ClientEventHandlers;

public sealed class CardDeletedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext) : IDomainEventHandler<CardDeletedDomainEvent>
{
    public Task Handle(CardDeletedDomainEvent domainEvent, CancellationToken cancellationToken)
        => hubContext.Clients.Group(domainEvent.BoardId).CardDeleted(new(domainEvent.Id), cancellationToken);
}
