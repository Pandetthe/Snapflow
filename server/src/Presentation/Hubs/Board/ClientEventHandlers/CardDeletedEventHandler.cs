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
        hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.CardDeleted(new(domainEvent.Id,
                showUsers
                    ? new IBoardHubClient.UserDto(domainEvent.DeletedById, domainEvent.DeletedByUserName,
                        avatarService.GenerateAvatarUrl(domainEvent.DeletedById))
                    : null), cancellationToken));
}
