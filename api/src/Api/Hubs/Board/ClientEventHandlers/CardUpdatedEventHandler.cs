using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Cards;

namespace Snapflow.Api.Hubs.Board.ClientEventHandlers;

public sealed class CardUpdatedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext) : IDomainEventHandler<CardUpdatedDomainEvent>
{
    public Task Handle(CardUpdatedDomainEvent domainEvent, CancellationToken cancellationToken)
        => hubContext.Clients.Group(domainEvent.BoardId).CardUpdated(new(domainEvent.Id, domainEvent.Title,
            domainEvent.Description), cancellationToken);
}