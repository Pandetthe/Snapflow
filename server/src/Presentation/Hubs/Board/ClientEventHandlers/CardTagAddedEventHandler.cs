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
        hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.CardTagAdded(new(domainEvent.CardId, domainEvent.TagId,
                showUsers
                    ? new IBoardHubClient.UserDto(domainEvent.AddedById, domainEvent.AddedByUserName,
                        avatarService.GenerateAvatarUrl(domainEvent.AddedById))
                    : null), cancellationToken));
}
