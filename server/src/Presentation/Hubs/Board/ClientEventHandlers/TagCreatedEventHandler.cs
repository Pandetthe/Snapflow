using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Tags;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class TagCreatedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    BoardHubUsers users) : IDomainEventHandler<TagCreatedDomainEvent>
{
    public async Task Handle(TagCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        IBoardHubClient.UserDto? createdBy = await users.FindAsync(domainEvent.CreatedById, cancellationToken);
        await hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.TagCreated(new(domainEvent.Id, domainEvent.Title, domainEvent.Color,
                showUsers ? createdBy : null), cancellationToken));
    }
}
