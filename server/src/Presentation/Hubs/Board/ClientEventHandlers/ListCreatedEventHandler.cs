using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Lists;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class ListCreatedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    BoardHubUsers users) : IDomainEventHandler<ListCreatedDomainEvent>
{
    public async Task Handle(ListCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        IBoardHubClient.UserDto? createdBy = await users.FindAsync(domainEvent.CreatedById, cancellationToken);
        await hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.ListCreated(new(domainEvent.Id, domainEvent.SwimlaneId, domainEvent.Title,
                domainEvent.Width, domainEvent.Rank,
                showUsers ? createdBy : null), cancellationToken));
    }
}
