using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Members.Add;
using Snapflow.Application.Members.ChangeOwner;
using Snapflow.Application.Members.ChangeRole;
using Snapflow.Application.Members.Remove;
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
        await TestApp.SucceedAsync(app.SendAsync(board.OwnerId, new AddMemberCommand(admin, board.BoardId, MemberRole.Admin)));

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
        await TestApp.SucceedAsync(app.SendAsync(board.OwnerId, new AddMemberCommand(successor, board.BoardId, MemberRole.Member)));

        await TestApp.SucceedAsync(app.SendAsync(board.OwnerId, new ChangeOwnerCommand(successor, board.BoardId)));

        Assert.Equal(MemberRole.Owner, await RoleAsync(app, board.BoardId, successor));
        Assert.Equal(MemberRole.Admin, await RoleAsync(app, board.BoardId, board.OwnerId));
    }

    [DockerFact]
    public async Task AddingAMemberTwice_IsAConflict()
    {
        await using TestApp app = TestApp.Create(fixture);
        TestBoard board = await app.CreateBoardAsync();
        int member = await app.CreateUserAsync();
        await TestApp.SucceedAsync(app.SendAsync(board.OwnerId, new AddMemberCommand(member, board.BoardId, MemberRole.Member)));

        Result again = await app.SendAsync(board.OwnerId, new AddMemberCommand(member, board.BoardId, MemberRole.Viewer));

        Assert.Equal("Members.AlreadyMember", again.Error.Code);
    }

    [DockerFact]
    public async Task RemovingAMember_DeletesTheMembership_ButTheOwnerStays()
    {
        await using TestApp app = TestApp.Create(fixture);
        TestBoard board = await app.CreateBoardAsync();
        int member = await app.CreateUserAsync();
        await TestApp.SucceedAsync(app.SendAsync(board.OwnerId, new AddMemberCommand(member, board.BoardId, MemberRole.Member)));

        await TestApp.SucceedAsync(app.SendAsync(board.OwnerId, new RemoveMemberCommand(board.BoardId, member)));
        Result ownerRemoved = await app.SendAsync(board.OwnerId, new RemoveMemberCommand(board.BoardId, board.OwnerId));

        Assert.Null(await RoleAsync(app, board.BoardId, member));
        Assert.Equal(MemberErrors.CannotRemoveOwner, ownerRemoved.Error);
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
