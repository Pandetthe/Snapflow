using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Tags;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class CardTagRemovedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    BoardHubUsers users) : IDomainEventHandler<CardTagRemovedDomainEvent>
{
    public async Task Handle(CardTagRemovedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        IBoardHubClient.UserDto? removedBy = await users.FindAsync(domainEvent.RemovedById, cancellationToken);
        await hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.CardTagRemoved(new(domainEvent.CardId, domainEvent.TagId,
                showUsers ? removedBy : null), cancellationToken));
    }
}
