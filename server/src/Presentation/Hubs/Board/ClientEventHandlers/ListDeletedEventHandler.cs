using Microsoft.AspNetCore.SignalR;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Lists;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class ListDeletedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext,
    IAvatarService avatarService) : IDomainEventHandler<ListDeletedDomainEvent>
{
    public Task Handle(ListDeletedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        hubContext.Clients
            .GroupExcept(domainEvent.BoardId, domainEvent.ConnectionId)
            .ListDeleted(new(domainEvent.Id,
                new(domainEvent.DeletedById, domainEvent.DeletedByUserName,
                    avatarService.GenerateAvatarUrl(domainEvent.DeletedById))), cancellationToken);
}
