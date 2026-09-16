using Microsoft.AspNetCore.SignalR;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class SwimlaneDeletedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    IAvatarService avatarService) : IDomainEventHandler<SwimlaneDeletedDomainEvent>
{
    public Task Handle(SwimlaneDeletedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.SwimlaneDeleted(new(domainEvent.Id,
                showUsers
                    ? new IBoardHubClient.UserDto(domainEvent.DeletedById, domainEvent.DeletedByUserName,
                        avatarService.GenerateAvatarUrl(domainEvent.DeletedById))
                    : null), cancellationToken));
}
