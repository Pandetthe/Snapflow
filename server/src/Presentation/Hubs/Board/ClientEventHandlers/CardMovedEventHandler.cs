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
        hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.CardMoved(new(domainEvent.Id, domainEvent.ListId, domainEvent.Rank,
                showUsers
                    ? new IBoardHubClient.UserDto(domainEvent.MovedById, domainEvent.MovedByUserName,
                        avatarService.GenerateAvatarUrl(domainEvent.MovedById))
                    : null), cancellationToken));
}