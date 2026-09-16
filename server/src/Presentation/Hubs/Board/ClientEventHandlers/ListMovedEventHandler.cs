using Microsoft.AspNetCore.SignalR;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Lists;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class ListMovedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    IAvatarService avatarService) : IDomainEventHandler<ListMovedDomainEvent>
{
    public Task Handle(ListMovedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.ListMoved(new(domainEvent.Id, domainEvent.SwimlaneId, domainEvent.Rank,
                showUsers
                    ? new IBoardHubClient.UserDto(domainEvent.MovedById, domainEvent.MovedByUserName,
                        avatarService.GenerateAvatarUrl(domainEvent.MovedById))
                    : null), cancellationToken));
}