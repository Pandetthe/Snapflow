using Microsoft.AspNetCore.SignalR;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Tags;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class CardTagAddedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    IAvatarService avatarService) : IDomainEventHandler<CardTagAddedDomainEvent>
{
    public Task Handle(CardTagAddedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        hubContext.Clients
            .GroupExcept(domainEvent.BoardId, domainEvent.ConnectionId)
            .CardTagAdded(new(domainEvent.CardId, domainEvent.TagId,
                new(domainEvent.AddedById, domainEvent.AddedByUserName,
                    avatarService.GenerateAvatarUrl(domainEvent.AddedById))), cancellationToken);
}
