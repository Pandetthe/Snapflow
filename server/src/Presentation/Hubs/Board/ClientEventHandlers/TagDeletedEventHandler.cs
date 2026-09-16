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
        hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.TagDeleted(new(domainEvent.Id,
                showUsers
                    ? new IBoardHubClient.UserDto(domainEvent.DeletedById, domainEvent.DeletedByUserName,
                        avatarService.GenerateAvatarUrl(domainEvent.DeletedById))
                    : null), cancellationToken));
}
