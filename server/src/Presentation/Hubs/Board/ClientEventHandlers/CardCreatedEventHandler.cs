using Microsoft.AspNetCore.SignalR;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Cards;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class CardCreatedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    IAvatarService avatarService) : IDomainEventHandler<CardCreatedDomainEvent>
{
    public Task Handle(CardCreatedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        hubContext.Clients.SendToBoard(domainEvent.BoardId, domainEvent.ConnectionId, (clients, showUsers) =>
            clients.CardCreated(new(domainEvent.Id, domainEvent.ListId, domainEvent.Title,
                domainEvent.Description, domainEvent.Rank, domainEvent.CreatedAt,
                showUsers
                    ? new IBoardHubClient.UserDto(domainEvent.CreatedById, domainEvent.CreatedByUserName,
                        avatarService.GenerateAvatarUrl(domainEvent.CreatedById))
                    : null), cancellationToken));
}
