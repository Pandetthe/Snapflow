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
        hubContext.Clients
            .GroupExcept(domainEvent.BoardId, domainEvent.ConnectionId)
            .ListMoved(new(domainEvent.Id, domainEvent.SwimlaneId, domainEvent.Rank,
                new(domainEvent.MovedById, domainEvent.MovedByUserName,
                    avatarService.GenerateAvatarUrl(domainEvent.MovedById))), cancellationToken);
}