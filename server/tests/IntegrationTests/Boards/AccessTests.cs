using Snapflow.Application.Boards.ChangeVisibility;
using Snapflow.Application.Boards.Delete;
using Snapflow.Application.Boards.GetDetails;
using Snapflow.Application.Members.Add;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Members;

namespace Snapflow.IntegrationTests.Boards;

[Collection(PostgresCollection.Name)]
public sealed class AccessTests(PostgresFixture fixture)
{
    [DockerFact]
    public async Task AFormerMemberOfADeletedBoard_IsToldItIsGone_WhileOthersAreRefused()
    {
        await using TestApp app = TestApp.Create(fixture);
        TestBoard board = await app.CreateBoardAsync();
        int stranger = await app.CreateUserAsync();
        await TestApp.SucceedAsync(app.SendAsync(board.OwnerId, new DeleteBoardCommand(board.BoardId)));

        Assert.True(await app.HasBoardPermissionAsync(board.OwnerId, board.BoardId, BoardPermissions.Boards.View));
        Assert.False(await app.HasBoardPermissionAsync(stranger, board.BoardId, BoardPermissions.Boards.View));

        Result<GetBoardDetailsResponse> details = await app.QueryAsync(board.OwnerId, new GetBoardDetailsQuery(board.BoardId));
        Assert.Equal(ErrorType.NotFound, details.Error.Type);
    }

    [DockerFact]
    public async Task OnlyTheOwner_CanChangeTheVisibility()
    {
        await using TestApp app = TestApp.Create(fixture);
        TestBoard board = await app.CreateBoardAsync();
        int admin = await app.CreateUserAsync();
        await TestApp.SucceedAsync(app.SendAsync(board.OwnerId, new AddMemberCommand(board.BoardId, admin, MemberRole.Admin)));

        Result byAdmin = await app.SendAsync(admin,
            new ChangeBoardVisibilityCommand(board.BoardId, BoardVisibility.Unlisted));
        Result byOwner = await app.SendAsync(board.OwnerId,
            new ChangeBoardVisibilityCommand(board.BoardId, BoardVisibility.Unlisted));

        Assert.Equal("Boards.VisibilityChangeForbidden", byAdmin.Error.Code);
        Assert.True(byOwner.IsSuccess, byOwner.IsFailure ? byOwner.Error.Code : null);
    }
}
