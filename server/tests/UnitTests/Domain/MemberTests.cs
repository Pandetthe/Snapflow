using Xunit;
using Snapflow.Common;
using FluentAssertions;
using Snapflow.Domain.Members;

namespace Snapflow.UnitTests.Domain;

public sealed class MemberTests
{
    [Fact]
    public void Create_Should_InitializeMember_And_RaiseEvent()
    {
        var boardId = 1;
        var userId = 2;
        var role = MemberRole.Admin;

        var member = Member.Create(boardId, userId, role, "conn-id");

        member.BoardId.Should().Be(boardId);
        member.UserId.Should().Be(userId);
        member.Role.Should().Be(role);

        var createdEvent = member.DomainEvents.Select(e => e(member))
            .OfType<MemberCreatedDomainEvent>()
            .Should().ContainSingle().Subject;
        
        createdEvent.Role.Should().Be(role);
        createdEvent.UserId.Should().Be(userId);
        createdEvent.BoardId.Should().Be(boardId);
    }

    [Fact]
    public void UpdateRole_Should_ChangeRole_And_RaiseEvent()
    {
        var member = Member.Create(1, 2, MemberRole.Viewer);
        var newRole = MemberRole.Admin;

        member.UpdateRole(newRole, "new-conn");

        member.Role.Should().Be(newRole);
        var updatedEvent = member.DomainEvents.Select(e => e(member))
            .OfType<MemberRoleChangedDomainEvent>()
            .Should().ContainSingle().Subject;
        
        updatedEvent.OldRole.Should().Be(MemberRole.Viewer);
        updatedEvent.NewRole.Should().Be(newRole);
    }

    [Fact]
    public void Remove_Should_RaiseEvent()
    {
        var member = Member.Create(1, 2, MemberRole.Admin);

        member.Remove("del-conn");

        var removedEvent = member.DomainEvents.Select(e => e(member))
            .OfType<MemberRemovedDomainEvent>()
            .Should().ContainSingle().Subject;
            
        removedEvent.UserId.Should().Be(2);
        removedEvent.BoardId.Should().Be(1);
    }
}
