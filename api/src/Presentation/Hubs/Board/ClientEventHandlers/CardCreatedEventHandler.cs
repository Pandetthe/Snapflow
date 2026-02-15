using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Cards;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class CardCreatedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext) : IDomainEventHandler<CardCreatedDomainEvent>
{
    public Task Handle(CardCreatedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        hubContext.Clients
            .GroupExcept(domainEvent.BoardId, domainEvent.ConnectionId)
            .CardCreated(new(domainEvent.Id, domainEvent.ListId, domainEvent.Title,
                domainEvent.Description, domainEvent.Rank), cancellationToken);
}
