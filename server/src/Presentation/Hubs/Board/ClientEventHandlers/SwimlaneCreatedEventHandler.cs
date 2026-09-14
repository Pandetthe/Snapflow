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
        hubContext.Clients
            .GroupExcept(domainEvent.BoardId, domainEvent.ConnectionId)
            .SwimlaneCreated(new(domainEvent.Id, domainEvent.Title,
                domainEvent.Height, domainEvent.Rank,
                new(domainEvent.CreatedById, domainEvent.CreatedByUserName,
                    avatarService.GenerateAvatarUrl(domainEvent.CreatedById))), cancellationToken);
}
