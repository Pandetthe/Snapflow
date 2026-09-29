using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Cards;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class CardDeletedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    BoardHubUsers users) : IDomainEventHandler<CardDeletedDomainEvent>
{
    public async Task Handle(CardDeletedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        IBoardHubClient.UserDto? deletedBy = await users.FindAsync(domainEvent.DeletedById, cancellationToken);
        await hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.CardDeleted(new(domainEvent.Id,
                showUsers ? deletedBy : null), cancellationToken));
    }
}
