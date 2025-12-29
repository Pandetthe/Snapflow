using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Members;

namespace Snapflow.Api.Hubs.Board.ClientEventHandlers;

public sealed class MemberRemovedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext) : IDomainEventHandler<MemberRemovedDomainEvent>
{
    public Task Handle(MemberRemovedDomainEvent domainEvent, CancellationToken cancellationToken)
        => hubContext.Clients.Group(domainEvent.BoardId).MemberRemoved(domainEvent.UserId, cancellationToken);
}
