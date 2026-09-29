using Snapflow.Application.Tags.Create;
using Snapflow.Application.Tags.Delete;
using Snapflow.Application.Tags.Update;
using Snapflow.Common;
using Snapflow.Domain.Tags;

namespace Snapflow.IntegrationTests.Tags;

[Collection(PostgresCollection.Name)]
public sealed class TagTests(PostgresFixture fixture)
{
    [DockerFact]
    public async Task ATitleIsUniqueOnItsBoard_UntilTheTagIsDeleted()
    {
        await using TestApp app = TestApp.Create(fixture);
        TestBoard board = await app.CreateBoardAsync();
        int bug = (await TestApp.SucceedAsync(app.SendAsync(board.OwnerId,
            new CreateTagCommand(board.BoardId, "Bug", TagColors.Red)))).Id;
        int feature = (await TestApp.SucceedAsync(app.SendAsync(board.OwnerId,
            new CreateTagCommand(board.BoardId, "Feature", TagColors.Blue)))).Id;

        Result<CreateTagResponse> duplicate = await app.SendAsync(board.OwnerId,
            new CreateTagCommand(board.BoardId, "Bug", TagColors.Green));
        Result<UpdateTagResponse> renamed = await app.SendAsync(board.OwnerId,
            new UpdateTagCommand(board.BoardId, feature, "Bug", TagColors.Blue));

        Assert.Equal("Tags.TitleNotUnique", duplicate.Error.Code);
        Assert.Equal("Tags.TitleNotUnique", renamed.Error.Code);

        await TestApp.SucceedAsync(app.SendAsync(board.OwnerId, new DeleteTagCommand(board.BoardId, bug)));
        await TestApp.SucceedAsync(app.SendAsync(board.OwnerId, new CreateTagCommand(board.BoardId, "Bug", TagColors.Green)));
    }

    [DockerFact]
    public async Task TheSameTitleCanBeUsedOnAnotherBoard()
    {
        await using TestApp app = TestApp.Create(fixture);
        TestBoard first = await app.CreateBoardAsync();
        TestBoard second = await app.CreateBoardAsync();

        await TestApp.SucceedAsync(app.SendAsync(first.OwnerId, new CreateTagCommand(first.BoardId, "Bug", TagColors.Red)));
        await TestApp.SucceedAsync(app.SendAsync(second.OwnerId, new CreateTagCommand(second.BoardId, "Bug", TagColors.Red)));
    }
}
