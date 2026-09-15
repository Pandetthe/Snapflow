using Microsoft.AspNetCore.SignalR;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Tags;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class TagDeletedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    IAvatarService avatarService) : IDomainEventHandler<TagDeletedDomainEvent>
{
    public Task Handle(TagDeletedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        hubContext.Clients
            .GroupExcept(domainEvent.BoardId, domainEvent.ConnectionId)
            .TagDeleted(new(domainEvent.Id,
                new(domainEvent.DeletedById, domainEvent.DeletedByUserName,
                    avatarService.GenerateAvatarUrl(domainEvent.DeletedById))), cancellationToken);
}
