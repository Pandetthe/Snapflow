using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class SwimlaneDeletedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    BoardHubUsers users) : IDomainEventHandler<SwimlaneDeletedDomainEvent>
{
    public async Task Handle(SwimlaneDeletedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        IBoardHubClient.UserDto? deletedBy = await users.FindAsync(domainEvent.DeletedById, cancellationToken);
        await hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.SwimlaneDeleted(new(domainEvent.Id,
                showUsers ? deletedBy : null), cancellationToken));
    }
}
