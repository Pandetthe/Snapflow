using Microsoft.AspNetCore.SignalR;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Lists;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class ListCreatedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    IAvatarService avatarService) : IDomainEventHandler<ListCreatedDomainEvent>
{
    public Task Handle(ListCreatedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.ListCreated(new(domainEvent.Id, domainEvent.SwimlaneId, domainEvent.Title,
                domainEvent.Width, domainEvent.Rank,
                showUsers
                    ? new IBoardHubClient.UserDto(domainEvent.CreatedById, domainEvent.CreatedByUserName,
                        avatarService.GenerateAvatarUrl(domainEvent.CreatedById))
                    : null), cancellationToken));
}
