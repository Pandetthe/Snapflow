using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Members.Add;
using Snapflow.Application.Members.ChangeOwner;
using Snapflow.Application.Members.ChangeRole;
using Snapflow.Application.Members.Remove;
using Snapflow.Application.Members.Replace;
using Snapflow.Common;
using Snapflow.Domain.Members;
using Snapflow.Infrastructure.Persistence;

namespace Snapflow.IntegrationTests.Members;

[Collection(PostgresCollection.Name)]
public sealed class MemberTests(PostgresFixture fixture)
{
    [DockerFact]
    public async Task AnAdmin_CannotChangeTheOwnersRole()
    {
        await using TestApp app = TestApp.Create(fixture);
        TestBoard board = await app.CreateBoardAsync();
        int admin = await app.CreateUserAsync();
        await TestApp.SucceedAsync(app.SendAsync(board.OwnerId, new AddMemberCommand(board.BoardId, admin, MemberRole.Admin)));

        Result demoted = await app.SendAsync(admin, new ChangeMemberRoleCommand(board.BoardId, board.OwnerId, MemberRole.Viewer));

        Assert.Equal(MemberErrors.CannotChangeOwnerRole, demoted.Error);
        Assert.Equal(MemberRole.Owner, await RoleAsync(app, board.BoardId, board.OwnerId));
    }

    [DockerFact]
    public async Task OwnershipMovesToAUserWithALowerId()
    {
        await using TestApp app = TestApp.Create(fixture);
        int successor = await app.CreateUserAsync();
        TestBoard board = await app.CreateBoardAsync();
        Assert.True(successor < board.OwnerId);
        await TestApp.SucceedAsync(app.SendAsync(board.OwnerId, new AddMemberCommand(board.BoardId, successor, MemberRole.Member)));

        await TestApp.SucceedAsync(app.SendAsync(board.OwnerId, new ChangeOwnerCommand(board.BoardId, successor)));

        Assert.Equal(MemberRole.Owner, await RoleAsync(app, board.BoardId, successor));
        Assert.Equal(MemberRole.Admin, await RoleAsync(app, board.BoardId, board.OwnerId));
    }

    [DockerFact]
    public async Task AddingAMemberTwice_IsAConflict()
    {
        await using TestApp app = TestApp.Create(fixture);
        TestBoard board = await app.CreateBoardAsync();
        int member = await app.CreateUserAsync();
        await TestApp.SucceedAsync(app.SendAsync(board.OwnerId, new AddMemberCommand(board.BoardId, member, MemberRole.Member)));

        Result again = await app.SendAsync(board.OwnerId, new AddMemberCommand(board.BoardId, member, MemberRole.Viewer));

        Assert.Equal("Members.AlreadyMember", again.Error.Code);
    }

    [DockerFact]
    public async Task RemovingAMember_DeletesTheMembership_ButTheOwnerStays()
    {
        await using TestApp app = TestApp.Create(fixture);
        TestBoard board = await app.CreateBoardAsync();
        int member = await app.CreateUserAsync();
        await TestApp.SucceedAsync(app.SendAsync(board.OwnerId, new AddMemberCommand(board.BoardId, member, MemberRole.Member)));

        await TestApp.SucceedAsync(app.SendAsync(board.OwnerId, new RemoveMemberCommand(board.BoardId, member)));
        Result ownerRemoved = await app.SendAsync(board.OwnerId, new RemoveMemberCommand(board.BoardId, board.OwnerId));

        Assert.Null(await RoleAsync(app, board.BoardId, member));
        Assert.Equal(MemberErrors.CannotRemoveOwner, ownerRemoved.Error);
    }

    [DockerFact]
    public async Task ReplacingTheMembers_AddsChangesAndRemoves_ButTheOwnerStays()
    {
        await using TestApp app = TestApp.Create(fixture);
        TestBoard board = await app.CreateBoardAsync();
        int kept = await app.CreateUserAsync();
        int dropped = await app.CreateUserAsync();
        int added = await app.CreateUserAsync();
        await TestApp.SucceedAsync(app.SendAsync(board.OwnerId, new AddMemberCommand(board.BoardId, kept, MemberRole.Viewer)));
        await TestApp.SucceedAsync(app.SendAsync(board.OwnerId, new AddMemberCommand(board.BoardId, dropped, MemberRole.Member)));

        await TestApp.SucceedAsync(app.SendAsync(board.OwnerId, new ReplaceMembersCommand(board.BoardId,
            [new ReplaceMemberRequest(kept, MemberRole.Admin), new ReplaceMemberRequest(added, MemberRole.Member)])));

        Assert.Equal(MemberRole.Owner, await RoleAsync(app, board.BoardId, board.OwnerId));
        Assert.Equal(MemberRole.Admin, await RoleAsync(app, board.BoardId, kept));
        Assert.Equal(MemberRole.Member, await RoleAsync(app, board.BoardId, added));
        Assert.Null(await RoleAsync(app, board.BoardId, dropped));
    }

    private static async Task<MemberRole?> RoleAsync(TestApp app, int boardId, int userId)
    {
        await using AppDbContext db = app.CreateDbContext();
        return await db.Members
            .Where(m => m.BoardId == boardId && m.UserId == userId)
            .Select(m => (MemberRole?)m.Role)
            .SingleOrDefaultAsync();
    }
}
