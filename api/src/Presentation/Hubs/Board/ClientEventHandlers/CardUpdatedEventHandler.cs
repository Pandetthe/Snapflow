using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Cards;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class CardUpdatedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext) : IDomainEventHandler<CardUpdatedDomainEvent>
{
    public Task Handle(CardUpdatedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        hubContext.Clients
            .GroupExcept(domainEvent.BoardId, domainEvent.ConnectionId)
            .CardUpdated(new(domainEvent.Id, domainEvent.Title, domainEvent.Description), cancellationToken);
}