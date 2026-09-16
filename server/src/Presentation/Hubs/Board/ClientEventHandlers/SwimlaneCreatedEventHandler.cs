using Microsoft.AspNetCore.SignalR;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class SwimlaneCreatedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    IAvatarService avatarService) : IDomainEventHandler<SwimlaneCreatedDomainEvent>
{
    public Task Handle(SwimlaneCreatedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.SwimlaneCreated(new(domainEvent.Id, domainEvent.Title,
                domainEvent.Height, domainEvent.Rank,
                showUsers
                    ? new IBoardHubClient.UserDto(domainEvent.CreatedById, domainEvent.CreatedByUserName,
                        avatarService.GenerateAvatarUrl(domainEvent.CreatedById))
                    : null), cancellationToken));
}
