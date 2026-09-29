using Snapflow.Application.Cards.Create;
using Snapflow.Application.Cards.GetByBoardId;
using Snapflow.Application.Cards.GetByListId;
using Snapflow.Application.Cards.GetBySwimlaneId;

namespace Snapflow.IntegrationTests.Cards;

[Collection(PostgresCollection.Name)]
public sealed class CardQueryTests(PostgresFixture fixture)
{
    [DockerFact]
    public async Task EveryCardQuery_ReportsTheCardsOwnListSwimlaneAndBoard()
    {
        await using TestApp app = TestApp.Create(fixture);
        TestBoard board = await app.CreateBoardAsync();
        await TestApp.SucceedAsync(app.SendAsync(board.OwnerId,
            new CreateCardCommand(board.BoardId, board.ListId, "Card", "", null)));

        var byBoard = Assert.Single(await TestApp.SucceedAsync(app.QueryAsync(board.OwnerId,
            new GetCardsByBoardIdQuery(board.BoardId))));
        var byList = Assert.Single(await TestApp.SucceedAsync(app.QueryAsync(board.OwnerId,
            new GetCardsByListIdQuery(board.BoardId, board.ListId))));
        var bySwimlane = Assert.Single(await TestApp.SucceedAsync(app.QueryAsync(board.OwnerId,
            new GetCardsBySwimlaneIdQuery(board.BoardId, board.SwimlaneId))));

        Assert.Equal((board.ListId, board.SwimlaneId, board.BoardId), (byBoard.ListId, byBoard.SwimlaneId, byBoard.BoardId));
        Assert.Equal((board.ListId, board.SwimlaneId, board.BoardId), (byList.ListId, byList.SwimlaneId, byList.BoardId));
        Assert.Equal((board.ListId, board.SwimlaneId, board.BoardId), (bySwimlane.ListId, bySwimlane.SwimlaneId, bySwimlane.BoardId));
    }
}
