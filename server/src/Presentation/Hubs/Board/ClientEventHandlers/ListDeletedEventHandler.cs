using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Lists;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class ListDeletedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    BoardHubUsers users) : IDomainEventHandler<ListDeletedDomainEvent>
{
    public async Task Handle(ListDeletedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        IBoardHubClient.UserDto? deletedBy = await users.FindAsync(domainEvent.DeletedById, cancellationToken);
        await hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.ListDeleted(new(domainEvent.Id,
                showUsers ? deletedBy : null), cancellationToken));
    }
}
