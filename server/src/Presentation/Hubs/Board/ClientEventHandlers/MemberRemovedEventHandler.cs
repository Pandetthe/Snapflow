using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Members;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class MemberRemovedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext) : IDomainEventHandler<MemberRemovedDomainEvent>
{
    public Task Handle(MemberRemovedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        hubContext.Clients
            .GroupExcept(domainEvent.BoardId, domainEvent.ConnectionId)
            .MemberRemoved(domainEvent.UserId, cancellationToken);
}
