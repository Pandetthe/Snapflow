using Microsoft.AspNetCore.SignalR;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Lists;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class ListDeletedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    IAvatarService avatarService) : IDomainEventHandler<ListDeletedDomainEvent>
{
    public Task Handle(ListDeletedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.ListDeleted(new(domainEvent.Id,
                showUsers
                    ? new IBoardHubClient.UserDto(domainEvent.DeletedById, domainEvent.DeletedByUserName,
                        avatarService.GenerateAvatarUrl(domainEvent.DeletedById))
                    : null), cancellationToken));
}
