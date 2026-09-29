using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Lists;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class ListUpdatedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    BoardHubUsers users) : IDomainEventHandler<ListUpdatedDomainEvent>
{
    public async Task Handle(ListUpdatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        IBoardHubClient.UserDto? updatedBy = await users.FindAsync(domainEvent.UpdatedById, cancellationToken);
        await hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.ListUpdated(new(domainEvent.Id, domainEvent.Title, domainEvent.Width,
                showUsers ? updatedBy : null), cancellationToken));
    }
}
