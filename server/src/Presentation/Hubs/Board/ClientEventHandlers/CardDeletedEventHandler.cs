using Microsoft.AspNetCore.SignalR;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Cards;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class CardDeletedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    IAvatarService avatarService) : IDomainEventHandler<CardDeletedDomainEvent>
{
    public Task Handle(CardDeletedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        hubContext.Clients
            .GroupExcept(domainEvent.BoardId, domainEvent.ConnectionId)
            .CardDeleted(new(domainEvent.Id,
                new(domainEvent.DeletedById, domainEvent.DeletedByUserName,
                    avatarService.GenerateAvatarUrl(domainEvent.DeletedById))), cancellationToken);
}
