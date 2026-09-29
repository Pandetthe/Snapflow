using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Cards.Create;
using Snapflow.Common;
using Snapflow.Infrastructure.Persistence;

namespace Snapflow.IntegrationTests.Cards;

[Collection(PostgresCollection.Name)]
public sealed class RankTests(PostgresFixture fixture)
{
    [DockerFact]
    public async Task InsertingAtTheTopUntilTheRankSpaceRunsOut_KeepsTheOrder()
    {
        await using TestApp app = TestApp.Create(fixture);
        TestBoard board = await app.CreateBoardAsync();
        int? firstId = null;

        for (int i = 0; i < 150; i++)
        {
            CreateCardResponse created = await TestApp.SucceedAsync(app.SendAsync(board.OwnerId,
                new CreateCardCommand(board.BoardId, board.ListId, $"Card {i}", "", firstId)));
            firstId = created.Id;
        }

        List<string> titles = await CardTitlesAsync(app, board.ListId);
        Assert.Equal(Enumerable.Range(0, 150).Reverse().Select(i => $"Card {i}"), titles);
    }

    [DockerFact]
    public async Task ParallelAppends_AllGetDistinctRanks()
    {
        await using TestApp app = TestApp.Create(fixture);
        TestBoard board = await app.CreateBoardAsync();

        Result<CreateCardResponse>[] results = await Task.WhenAll(Enumerable.Range(0, 20).Select(i =>
            app.SendAsync(board.OwnerId, new CreateCardCommand(board.BoardId, board.ListId, $"Card {i}", "", null))));

        Assert.All(results, result => Assert.True(result.IsSuccess, result.IsFailure ? result.Error.Code : null));
        Assert.Equal(20, results.Select(r => r.Value.Rank).Distinct().Count());
    }

    [DockerFact]
    public async Task InsertingIntoAListWithNoRoomAroundTheTarget_NormalisesTheWholeList()
    {
        await using TestApp app = TestApp.Create(fixture);
        TestBoard board = await app.CreateBoardAsync();
        await using (AppDbContext db = app.CreateDbContext())
        {
            await db.Database.ExecuteSqlAsync($"""
                INSERT INTO cards (board_id, swimlane_id, list_id, title, description, rank, created_at, created_by_id, is_deleted, deleted_by_cascade)
                SELECT {board.BoardId}, {board.SwimlaneId}, {board.ListId}, 'D' || n, '', lpad(n::text, 12, '0'), now(), {board.OwnerId}, false, false
                FROM generate_series(1, 30) AS n
                """);
        }

        int fifteenth = await CardIdAsync(app, board.ListId, "D15");
        await TestApp.SucceedAsync(app.SendAsync(board.OwnerId,
            new CreateCardCommand(board.BoardId, board.ListId, "Middle", "", fifteenth)));

        List<string> titles = await CardTitlesAsync(app, board.ListId);
        Assert.Equal(31, titles.Count);
        Assert.Equal(14, titles.IndexOf("Middle"));
    }

    private static async Task<List<string>> CardTitlesAsync(TestApp app, int listId)
    {
        await using AppDbContext db = app.CreateDbContext();
        return await db.Cards.Where(c => c.ListId == listId).OrderBy(c => c.Rank).Select(c => c.Title).ToListAsync();
    }

    private static async Task<int> CardIdAsync(TestApp app, int listId, string title)
    {
        await using AppDbContext db = app.CreateDbContext();
        return await db.Cards.Where(c => c.ListId == listId && c.Title == title).Select(c => c.Id).SingleAsync();
    }
}
