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

        Assert.True(result.IsSuccess);
        var member = Assert.Single(board.Members, m => m.UserId == 2);
        Assert.Equal(MemberRole.Admin, member.Role);
        MemberCreatedDomainEvent raised = Assert.Single(Events<MemberCreatedDomainEvent>(member));
        Assert.True(raised.Role == MemberRole.Admin && raised.ConnectionId == "conn-id");
    }

    [Fact]
    public void AddMember_Should_Fail_When_UserIsAlreadyMember()
    {
        var board = CreateBoard();
        board.AddMember(2, MemberRole.Member);

        var result = board.AddMember(2, MemberRole.Viewer);

        Assert.Equal("Members.AlreadyMember", result.Error.Code);
        Assert.Equal(2, board.Members.Count);
    }

    [Fact]
    public void AddMember_Should_Fail_When_RoleIsOwner()
    {
        var board = CreateBoard();

        var result = board.AddMember(2, MemberRole.Owner);

        Assert.Equal(MemberErrors.CannotAssignOwner, result.Error);
        Assert.Single(board.Members);
    }

    [Fact]
    public void ChangeMemberRole_Should_ChangeRole_And_RaiseEvent()
    {
        var board = CreateBoard();
        board.AddMember(2, MemberRole.Member);

        var result = board.ChangeMemberRole(2, MemberRole.Admin, "new-conn");

        Assert.True(result.IsSuccess);
        var member = board.Members.Single(m => m.UserId == 2);
        Assert.Equal(MemberRole.Admin, member.Role);
        MemberRoleChangedDomainEvent raised = Assert.Single(Events<MemberRoleChangedDomainEvent>(member));
        Assert.True(raised.OldRole == MemberRole.Member && raised.NewRole == MemberRole.Admin && raised.ConnectionId == "new-conn");
    }

    [Fact]
    public void ChangeMemberRole_Should_Fail_When_TargetIsOwner()
    {
        var board = CreateBoard();

        var result = board.ChangeMemberRole(OwnerId, MemberRole.Viewer);

        Assert.Equal(MemberErrors.CannotChangeOwnerRole, result.Error);
        Assert.Equal(MemberRole.Owner, board.Members.Single().Role);
    }

    [Fact]
    public void ChangeMemberRole_Should_Fail_When_NewRoleIsOwner()
    {
        var board = CreateBoard();
        board.AddMember(2, MemberRole.Admin);

        var result = board.ChangeMemberRole(2, MemberRole.Owner);

        Assert.Equal(MemberErrors.CannotAssignOwner, result.Error);
    }

    [Fact]
    public void RemoveMember_Should_RemoveMember_And_RaiseEvent()
    {
        var board = CreateBoard();
        board.AddMember(2, MemberRole.Member);
        var member = board.Members.Single(m => m.UserId == 2);

        var result = board.RemoveMember(2, "del-conn");

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(member, board.Members);
        Assert.Equal("del-conn", Assert.Single(Events<MemberRemovedDomainEvent>(member)).ConnectionId);
    }

    [Fact]
    public void RemoveMember_Should_Fail_When_TargetIsOwner()
    {
        var board = CreateBoard();

        var result = board.RemoveMember(OwnerId);

        Assert.Equal(MemberErrors.CannotRemoveOwner, result.Error);
        Assert.Single(board.Members);
    }

    [Fact]
    public void HandOverAndTakeOwnership_Should_SwapOwnerAndAdmin()
    {
        var board = CreateBoard();
        board.AddMember(2, MemberRole.Member);

        var handedOver = board.HandOverOwnership(2);
        var taken = board.TakeOwnership(2);

        Assert.True(handedOver.IsSuccess);
        Assert.True(taken.IsSuccess);
        Assert.Equal(MemberRole.Admin, board.Members.Single(m => m.UserId == OwnerId).Role);
        Assert.Equal(MemberRole.Owner, board.Members.Single(m => m.UserId == 2).Role);
        Assert.True(board.IsOwnedBy(2));
    }

    [Fact]
    public void HandOverOwnership_Should_Fail_When_SuccessorIsNotMember()
    {
        var board = CreateBoard();

        var result = board.HandOverOwnership(2);

        Assert.Equal("Members.NotFound", result.Error.Code);
        Assert.True(board.IsOwnedBy(OwnerId));
    }

    [Fact]
    public void HandOverOwnership_Should_Fail_When_SuccessorAlreadyOwns()
    {
        var board = CreateBoard();

        var result = board.HandOverOwnership(OwnerId);

        Assert.Equal("Members.AlreadyOwner", result.Error.Code);
    }

    [Fact]
    public void TakeOwnership_Should_Fail_While_BoardHasOwner()
    {
        var board = CreateBoard();
        board.AddMember(2, MemberRole.Admin);

        var result = board.TakeOwnership(2);

        Assert.Equal("Members.OwnerAlreadyExists", result.Error.Code);
        Assert.True(board.IsOwnedBy(OwnerId));
    }

    [Fact]
    public void SyncMembers_Should_AddChangeAndRemove_And_KeepOwner()
    {
        var board = CreateBoard();
        board.AddMember(2, MemberRole.Member);
        board.AddMember(3, MemberRole.Viewer);

        var result = board.SyncMembers([(2, MemberRole.Admin), (4, MemberRole.Viewer), (OwnerId, MemberRole.Viewer)]);

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { (OwnerId, MemberRole.Owner), (2, MemberRole.Admin), (4, MemberRole.Viewer) }.Order(), board.Members.Select(m => (m.UserId, m.Role)).Order());
    }

    [Fact]
    public void SyncMembers_Should_Fail_When_UserIsRequestedTwice()
    {
        var board = CreateBoard();

        var result = board.SyncMembers([(2, MemberRole.Admin), (2, MemberRole.Viewer)]);

        Assert.Equal(MemberErrors.DuplicateMember, result.Error);
        Assert.Single(board.Members);
    }
}
