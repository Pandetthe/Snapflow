using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Tags;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class TagUpdatedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    BoardHubUsers users) : IDomainEventHandler<TagUpdatedDomainEvent>
{
    public async Task Handle(TagUpdatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        IBoardHubClient.UserDto? updatedBy = await users.FindAsync(domainEvent.UpdatedById, cancellationToken);
        await hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.TagUpdated(new(domainEvent.Id, domainEvent.Title, domainEvent.Color,
                showUsers ? updatedBy : null), cancellationToken));
    }
}
