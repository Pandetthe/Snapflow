using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class SwimlaneUpdatedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    BoardHubUsers users) : IDomainEventHandler<SwimlaneUpdatedDomainEvent>
{
    public async Task Handle(SwimlaneUpdatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        IBoardHubClient.UserDto? updatedBy = await users.FindAsync(domainEvent.UpdatedById, cancellationToken);
        await hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.SwimlaneUpdated(new(domainEvent.Id, domainEvent.Title,
                domainEvent.Height,
                showUsers ? updatedBy : null), cancellationToken));
    }
}
