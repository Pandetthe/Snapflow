using Microsoft.AspNetCore.SignalR;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Tags;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class CardTagRemovedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    IAvatarService avatarService) : IDomainEventHandler<CardTagRemovedDomainEvent>
{
    public Task Handle(CardTagRemovedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        hubContext.Clients
            .GroupExcept(domainEvent.BoardId, domainEvent.ConnectionId)
            .CardTagRemoved(new(domainEvent.CardId, domainEvent.TagId,
                new(domainEvent.RemovedById, domainEvent.RemovedByUserName,
                    avatarService.GenerateAvatarUrl(domainEvent.RemovedById))), cancellationToken);
}
