using Microsoft.AspNetCore.SignalR;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Cards;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class CardMovedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    IAvatarService avatarService) : IDomainEventHandler<CardMovedDomainEvent>
{
    public Task Handle(CardMovedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        hubContext.Clients
            .GroupExcept(domainEvent.BoardId, domainEvent.ConnectionId)
            .CardMoved(new(domainEvent.Id, domainEvent.ListId, domainEvent.Rank,
                new(domainEvent.MovedById, domainEvent.MovedByUserName,
                    avatarService.GenerateAvatarUrl(domainEvent.MovedById))), cancellationToken);
}