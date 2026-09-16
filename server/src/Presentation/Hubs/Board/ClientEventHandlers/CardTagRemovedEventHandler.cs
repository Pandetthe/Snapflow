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
        hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.CardTagRemoved(new(domainEvent.CardId, domainEvent.TagId,
                showUsers
                    ? new IBoardHubClient.UserDto(domainEvent.RemovedById, domainEvent.RemovedByUserName,
                        avatarService.GenerateAvatarUrl(domainEvent.RemovedById))
                    : null), cancellationToken));
}
