using Microsoft.AspNetCore.SignalR;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Tags;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class TagCreatedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    IAvatarService avatarService) : IDomainEventHandler<TagCreatedDomainEvent>
{
    public Task Handle(TagCreatedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        hubContext.Clients
            .GroupExcept(domainEvent.BoardId, domainEvent.ConnectionId)
            .TagCreated(new(domainEvent.Id, domainEvent.Title, domainEvent.Color,
                new(domainEvent.CreatedById, domainEvent.CreatedByUserName,
                    avatarService.GenerateAvatarUrl(domainEvent.CreatedById))), cancellationToken);
}
