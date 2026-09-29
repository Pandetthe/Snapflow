using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Cards;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class CardMovedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    BoardHubUsers users) : IDomainEventHandler<CardMovedDomainEvent>
{
    public async Task Handle(CardMovedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        IBoardHubClient.UserDto? movedBy = await users.FindAsync(domainEvent.MovedById, cancellationToken);
        await hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.CardMoved(new(domainEvent.Id, domainEvent.ListId, domainEvent.Rank,
                showUsers ? movedBy : null), cancellationToken));
    }
}
