using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class SwimlaneCreatedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    BoardHubUsers users) : IDomainEventHandler<SwimlaneCreatedDomainEvent>
{
    public async Task Handle(SwimlaneCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        IBoardHubClient.UserDto? createdBy = await users.FindAsync(domainEvent.CreatedById, cancellationToken);
        await hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.SwimlaneCreated(new(domainEvent.Id, domainEvent.Title,
                domainEvent.Height, domainEvent.Rank,
                showUsers ? createdBy : null), cancellationToken));
    }
}
