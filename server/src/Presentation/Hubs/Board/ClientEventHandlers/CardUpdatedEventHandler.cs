using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Cards;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class CardUpdatedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    BoardHubUsers users) : IDomainEventHandler<CardUpdatedDomainEvent>
{
    public async Task Handle(CardUpdatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        IBoardHubClient.UserDto? updatedBy = await users.FindAsync(domainEvent.UpdatedById, cancellationToken);
        await hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.CardUpdated(new(domainEvent.Id, domainEvent.Title, domainEvent.Description,
                showUsers ? updatedBy : null), cancellationToken));
    }
}
