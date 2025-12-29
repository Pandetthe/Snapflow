using Microsoft.AspNetCore.SignalR;
using Snapflow.Common;
using Snapflow.Domain.Members;

namespace Snapflow.Api.Hubs.Board.ClientEventHandlers;

public sealed class MemberRoleChangedEventHandler(
    IHubContext<BoardHub, IBoardHubClient> hubContext) : IDomainEventHandler<MemberRoleChangedDomainEvent>
{
    public Task Handle(MemberRoleChangedDomainEvent domainEvent, CancellationToken cancellationToken) =>
        hubContext.Clients.Group(domainEvent.BoardId, domainEvent.UserId)
            .YourRoleChanged(domainEvent.OldRole, domainEvent.NewRole, cancellationToken);
}
