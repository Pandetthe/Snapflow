using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Tags;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class CardTagAddedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    BoardHubUsers users) : IDomainEventHandler<CardTagAddedDomainEvent>
{
    public async Task Handle(CardTagAddedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        IBoardHubClient.UserDto? addedBy = await users.FindAsync(domainEvent.AddedById, cancellationToken);
        await hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.CardTagAdded(new(domainEvent.CardId, domainEvent.TagId,
                showUsers ? addedBy : null), cancellationToken));
    }
}
