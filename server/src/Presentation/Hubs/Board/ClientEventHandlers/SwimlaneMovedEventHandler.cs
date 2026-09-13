using Microsoft.AspNetCore.SignalR;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class SwimlaneMovedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    IAvatarService avatarService) : IDomainEventHandler<SwimlaneMovedDomainEvent>
{
    public Task Handle(SwimlaneMovedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        hubContext.Clients
            .GroupExcept(domainEvent.BoardId, domainEvent.ConnectionId)
            .SwimlaneMoved(new(domainEvent.Id, domainEvent.Rank,
                new(domainEvent.MovedById, domainEvent.MovedByUserName,
                    avatarService.GenerateAvatarUrl(domainEvent.MovedById))), cancellationToken);
}