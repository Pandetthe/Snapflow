using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Cards;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class CardCreatedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    BoardHubUsers users) : IDomainEventHandler<CardCreatedDomainEvent>
{
    public async Task Handle(CardCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        IBoardHubClient.UserDto? createdBy = await users.FindAsync(domainEvent.CreatedById, cancellationToken);
        await hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.CardCreated(new(domainEvent.Id, domainEvent.ListId, domainEvent.Title,
                domainEvent.Description, domainEvent.Rank, domainEvent.CreatedAt,
                showUsers ? createdBy : null), cancellationToken));
    }
}
