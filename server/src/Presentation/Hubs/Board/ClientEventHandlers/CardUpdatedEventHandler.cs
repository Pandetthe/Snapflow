using Microsoft.AspNetCore.SignalR;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Cards;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class CardUpdatedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    IAvatarService avatarService) : IDomainEventHandler<CardUpdatedDomainEvent>
{
    public Task Handle(CardUpdatedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.CardUpdated(new(domainEvent.Id, domainEvent.Title, domainEvent.Description,
                showUsers
                    ? new IBoardHubClient.UserDto(domainEvent.UpdatedById, domainEvent.UpdatedByUserName,
                        avatarService.GenerateAvatarUrl(domainEvent.UpdatedById))
                    : null), cancellationToken));
}
