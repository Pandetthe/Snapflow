using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Cards;

namespace Snapflow.Api.Hubs.Board.ClientEventHandlers;

public sealed class CardCreatedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext) : IDomainEventHandler<CardCreatedDomainEvent>
{
    public Task Handle(CardCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
        => hubContext.Clients.Group(domainEvent.BoardId).CardCreated(
            domainEvent.Id, domainEvent.ListId, domainEvent.SwimlaneId,
            domainEvent.Title, domainEvent.Description, cancellationToken);
}
