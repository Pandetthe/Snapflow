using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Tags;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class TagDeletedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    BoardHubUsers users) : IDomainEventHandler<TagDeletedDomainEvent>
{
    public async Task Handle(TagDeletedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        IBoardHubClient.UserDto? deletedBy = await users.FindAsync(domainEvent.DeletedById, cancellationToken);
        await hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.TagDeleted(new(domainEvent.Id,
                showUsers ? deletedBy : null), cancellationToken));
    }
}
