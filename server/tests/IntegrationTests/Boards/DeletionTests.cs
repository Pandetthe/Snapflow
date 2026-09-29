using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Boards.Delete;
using Snapflow.Application.Cards.Create;
using Snapflow.Application.Cards.Delete;
using Snapflow.Application.Cards.GetByListId;
using Snapflow.Application.Lists.Move;
using Snapflow.Application.Swimlanes.Create;
using Snapflow.Application.Swimlanes.Delete;
using Snapflow.Application.Tags.Create;
using Snapflow.Common;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Tags;
using Snapflow.Infrastructure.Persistence;

namespace Snapflow.IntegrationTests.Boards;

[Collection(PostgresCollection.Name)]
public sealed class DeletionTests(PostgresFixture fixture)
{
    [DockerFact]
    public async Task DeletingTheOldSwimlane_KeepsTheCardsOfAListMovedAwayFromIt()
    {
        await using TestApp app = TestApp.Create(fixture);
        TestBoard board = await app.CreateBoardAsync();
        int target = (await TestApp.SucceedAsync(app.SendAsync(board.OwnerId,
            new CreateSwimlaneCommand(board.BoardId, "Target", null, null)))).Id;
        await TestApp.SucceedAsync(app.SendAsync(board.OwnerId,
            new CreateCardCommand(board.BoardId, board.ListId, "Card", "", null)));

        await TestApp.SucceedAsync(app.SendAsync(board.OwnerId, new MoveListCommand(board.BoardId, board.ListId, target, null)));
        await TestApp.SucceedAsync(app.SendAsync(board.OwnerId, new DeleteSwimlaneCommand(board.BoardId, board.SwimlaneId)));

        var cards = await TestApp.SucceedAsync(app.QueryAsync(board.OwnerId, new GetCardsByListIdQuery(board.BoardId, board.ListId)));
        var card = Assert.Single(cards);
        Assert.Equal(target, card.SwimlaneId);

        await TestApp.SucceedAsync(app.SendAsync(board.OwnerId, new DeleteSwimlaneCommand(board.BoardId, target)));
        Result<IReadOnlyList<GetCardsByListIdResponse.CardDto>> afterTargetDeleted =
            await app.QueryAsync(board.OwnerId, new GetCardsByListIdQuery(board.BoardId, board.ListId));
        Assert.True(afterTargetDeleted.IsFailure);
    }

    [DockerFact]
    public async Task DeletingABoard_CascadesToEverythingOnIt_AndKeepsEarlierDeletions()
    {
        await using TestApp app = TestApp.Create(fixture);
        TestBoard board = await app.CreateBoardAsync();
        int kept = (await TestApp.SucceedAsync(app.SendAsync(board.OwnerId,
            new CreateCardCommand(board.BoardId, board.ListId, "Kept", "", null)))).Id;
        int deletedEarlier = (await TestApp.SucceedAsync(app.SendAsync(board.OwnerId,
            new CreateCardCommand(board.BoardId, board.ListId, "Gone", "", null)))).Id;
        await TestApp.SucceedAsync(app.SendAsync(board.OwnerId,
            new CreateTagCommand(board.BoardId, "Bug", TagColors.Red)));
        await TestApp.SucceedAsync(app.SendAsync(board.OwnerId, new DeleteCardCommand(board.BoardId, deletedEarlier)));

        await TestApp.SucceedAsync(app.SendAsync(board.OwnerId, new DeleteBoardCommand(board.BoardId)));

        await using AppDbContext db = app.CreateDbContext();
        var boardRow = await db.Boards.IgnoreQueryFilters().SingleAsync(b => b.Id == board.BoardId);
        var swimlane = await db.Swimlanes.IgnoreQueryFilters().SingleAsync(s => s.Id == board.SwimlaneId);
        var list = await db.Lists.IgnoreQueryFilters().SingleAsync(l => l.Id == board.ListId);
        Card keptCard = await db.Cards.IgnoreQueryFilters().SingleAsync(c => c.Id == kept);
        Card earlierCard = await db.Cards.IgnoreQueryFilters().SingleAsync(c => c.Id == deletedEarlier);
        Tag tag = await db.Tags.IgnoreQueryFilters().SingleAsync(t => t.BoardId == board.BoardId);

        Assert.True(boardRow.IsDeleted);
        Assert.True(swimlane is { IsDeleted: true, DeletedByCascade: true });
        Assert.True(list is { IsDeleted: true, DeletedByCascade: true });
        Assert.True(keptCard is { IsDeleted: true, DeletedByCascade: true });
        Assert.True(tag.IsDeleted);
        Assert.True(earlierCard is { IsDeleted: true, DeletedByCascade: false });
        Assert.True(earlierCard.DeletedAt < keptCard.DeletedAt);
        Assert.False(await db.Members.AnyAsync(m => m.BoardId == board.BoardId));
    }
}
