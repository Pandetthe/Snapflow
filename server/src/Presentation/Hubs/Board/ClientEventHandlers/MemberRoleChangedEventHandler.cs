using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Members;

namespace Snapflow.Presentation.Hubs.Board.ClientEventHandlers;

public sealed class MemberRoleChangedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext) : IDomainEventHandler<MemberRoleChangedDomainEvent>
{
    public Task Handle(MemberRoleChangedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        hubContext.Clients
            .GroupExcept(domainEvent.BoardId, domainEvent.UserId, domainEvent.ConnectionId)
            .YourRoleChanged(domainEvent.OldRole, domainEvent.NewRole, cancellationToken);
}
