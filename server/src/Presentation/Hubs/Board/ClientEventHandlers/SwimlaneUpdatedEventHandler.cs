using Microsoft.AspNetCore.SignalR;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class SwimlaneUpdatedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    IAvatarService avatarService) : IDomainEventHandler<SwimlaneUpdatedDomainEvent>
{
    public Task Handle(SwimlaneUpdatedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.SwimlaneUpdated(new(domainEvent.Id, domainEvent.Title,
                domainEvent.Height,
                showUsers
                    ? new IBoardHubClient.UserDto(domainEvent.UpdatedById, domainEvent.UpdatedByUserName,
                        avatarService.GenerateAvatarUrl(domainEvent.UpdatedById))
                    : null), cancellationToken));
}
