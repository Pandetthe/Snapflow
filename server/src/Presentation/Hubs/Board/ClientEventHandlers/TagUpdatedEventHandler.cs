using Microsoft.AspNetCore.SignalR;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Tags;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class TagUpdatedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    IAvatarService avatarService) : IDomainEventHandler<TagUpdatedDomainEvent>
{
    public Task Handle(TagUpdatedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.TagUpdated(new(domainEvent.Id, domainEvent.Title, domainEvent.Color,
                showUsers
                    ? new IBoardHubClient.UserDto(domainEvent.UpdatedById, domainEvent.UpdatedByUserName,
                        avatarService.GenerateAvatarUrl(domainEvent.UpdatedById))
                    : null), cancellationToken));
}
