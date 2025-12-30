using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Cards;

namespace Snapflow.Api.Hubs.Board.ClientEventHandlers;

public sealed class CardMovedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext) : IDomainEventHandler<CardMovedDomainEvent>
{
    public Task Handle(CardMovedDomainEvent domainEvent, CancellationToken cancellationToken)
        => hubContext.Clients.Group(domainEvent.BoardId).CardMoved(new(domainEvent.Id, domainEvent.ListId, domainEvent.Rank), cancellationToken);
}