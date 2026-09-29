using FluentAssertions;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Members;

namespace Snapflow.UnitTests.Domain;

public sealed class MemberTests
{
    private const int OwnerId = 1;

    private static Board CreateBoard() =>
        Board.Create("Board", "", BoardVisibility.Private, OwnerId, DateTimeOffset.UtcNow);

    private static IEnumerable<T> Events<T>(Member member) =>
        member.DomainEvents.Select(e => e(member)).OfType<T>();

    [Fact]
    public void AddMember_Should_AddMember_And_RaiseEvent()
    {
        var board = CreateBoard();

        var result = board.AddMember(2, MemberRole.Admin, "conn-id");

        result.IsSuccess.Should().BeTrue();
        var member = board.Members.Should().ContainSingle(m => m.UserId == 2).Subject;
        member.Role.Should().Be(MemberRole.Admin);
        Events<MemberCreatedDomainEvent>(member).Should().ContainSingle()
            .Which.Should().Match<MemberCreatedDomainEvent>(e => e.Role == MemberRole.Admin && e.ConnectionId == "conn-id");
    }

    [Fact]
    public void AddMember_Should_Fail_When_UserIsAlreadyMember()
    {
        var board = CreateBoard();
        board.AddMember(2, MemberRole.Member);

        var result = board.AddMember(2, MemberRole.Viewer);

        result.Error.Code.Should().Be("Members.AlreadyMember");
        board.Members.Should().HaveCount(2);
    }

    [Fact]
    public void AddMember_Should_Fail_When_RoleIsOwner()
    {
        var board = CreateBoard();

        var result = board.AddMember(2, MemberRole.Owner);

        result.Error.Should().Be(MemberErrors.CannotAssignOwner);
        board.Members.Should().ContainSingle();
    }

    [Fact]
    public void ChangeMemberRole_Should_ChangeRole_And_RaiseEvent()
    {
        var board = CreateBoard();
        board.AddMember(2, MemberRole.Member);

        var result = board.ChangeMemberRole(2, MemberRole.Admin, "new-conn");

        result.IsSuccess.Should().BeTrue();
        var member = board.Members.Single(m => m.UserId == 2);
        member.Role.Should().Be(MemberRole.Admin);
        Events<MemberRoleChangedDomainEvent>(member).Should().ContainSingle()
            .Which.Should().Match<MemberRoleChangedDomainEvent>(e =>
                e.OldRole == MemberRole.Member && e.NewRole == MemberRole.Admin && e.ConnectionId == "new-conn");
    }

    [Fact]
    public void ChangeMemberRole_Should_Fail_When_TargetIsOwner()
    {
        var board = CreateBoard();

        var result = board.ChangeMemberRole(OwnerId, MemberRole.Viewer);

        result.Error.Should().Be(MemberErrors.CannotChangeOwnerRole);
        board.Members.Single().Role.Should().Be(MemberRole.Owner);
    }

    [Fact]
    public void ChangeMemberRole_Should_Fail_When_NewRoleIsOwner()
    {
        var board = CreateBoard();
        board.AddMember(2, MemberRole.Admin);

        var result = board.ChangeMemberRole(2, MemberRole.Owner);

        result.Error.Should().Be(MemberErrors.CannotAssignOwner);
    }

    [Fact]
    public void RemoveMember_Should_RemoveMember_And_RaiseEvent()
    {
        var board = CreateBoard();
        board.AddMember(2, MemberRole.Member);
        var member = board.Members.Single(m => m.UserId == 2);

        var result = board.RemoveMember(2, "del-conn");

        result.IsSuccess.Should().BeTrue();
        board.Members.Should().NotContain(member);
        Events<MemberRemovedDomainEvent>(member).Should().ContainSingle()
            .Which.ConnectionId.Should().Be("del-conn");
    }

    [Fact]
    public void RemoveMember_Should_Fail_When_TargetIsOwner()
    {
        var board = CreateBoard();

        var result = board.RemoveMember(OwnerId);

        result.Error.Should().Be(MemberErrors.CannotRemoveOwner);
        board.Members.Should().ContainSingle();
    }

    [Fact]
    public void HandOverAndTakeOwnership_Should_SwapOwnerAndAdmin()
    {
        var board = CreateBoard();
        board.AddMember(2, MemberRole.Member);

        var handedOver = board.HandOverOwnership(2);
        var taken = board.TakeOwnership(2);

        handedOver.IsSuccess.Should().BeTrue();
        taken.IsSuccess.Should().BeTrue();
        board.Members.Single(m => m.UserId == OwnerId).Role.Should().Be(MemberRole.Admin);
        board.Members.Single(m => m.UserId == 2).Role.Should().Be(MemberRole.Owner);
        board.IsOwnedBy(2).Should().BeTrue();
    }

    [Fact]
    public void HandOverOwnership_Should_Fail_When_SuccessorIsNotMember()
    {
        var board = CreateBoard();

        var result = board.HandOverOwnership(2);

        result.Error.Code.Should().Be("Members.NotFound");
        board.IsOwnedBy(OwnerId).Should().BeTrue();
    }

    [Fact]
    public void HandOverOwnership_Should_Fail_When_SuccessorAlreadyOwns()
    {
        var board = CreateBoard();

        var result = board.HandOverOwnership(OwnerId);

        result.Error.Code.Should().Be("Members.AlreadyOwner");
    }

    [Fact]
    public void TakeOwnership_Should_Fail_While_BoardHasOwner()
    {
        var board = CreateBoard();
        board.AddMember(2, MemberRole.Admin);

        var result = board.TakeOwnership(2);

        result.Error.Code.Should().Be("Members.OwnerAlreadyExists");
        board.IsOwnedBy(OwnerId).Should().BeTrue();
    }

    [Fact]
    public void SyncMembers_Should_AddChangeAndRemove_And_KeepOwner()
    {
        var board = CreateBoard();
        board.AddMember(2, MemberRole.Member);
        board.AddMember(3, MemberRole.Viewer);

        var result = board.SyncMembers([(2, MemberRole.Admin), (4, MemberRole.Viewer), (OwnerId, MemberRole.Viewer)]);

        result.IsSuccess.Should().BeTrue();
        board.Members.Select(m => (m.UserId, m.Role)).Should().BeEquivalentTo(
            [(OwnerId, MemberRole.Owner), (2, MemberRole.Admin), (4, MemberRole.Viewer)]);
    }

    [Fact]
    public void SyncMembers_Should_Fail_When_UserIsRequestedTwice()
    {
        var board = CreateBoard();

        var result = board.SyncMembers([(2, MemberRole.Admin), (2, MemberRole.Viewer)]);

        result.Error.Should().Be(MemberErrors.DuplicateMember);
        board.Members.Should().ContainSingle();
    }
}
