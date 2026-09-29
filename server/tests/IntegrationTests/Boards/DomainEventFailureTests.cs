using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Snapflow.Application.Boards.Update;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Infrastructure.Persistence;

namespace Snapflow.IntegrationTests.Boards;

[Collection(PostgresCollection.Name)]
public sealed class DomainEventFailureTests(PostgresFixture fixture)
{
    [DockerFact]
    public async Task AFailingEventHandler_DoesNotMakeACommittedChangeLookFailed()
    {
        await using TestApp app = TestApp.Create(fixture, services => services
            .AddScoped<IDomainEventHandler<BoardUpdatedDomainEvent>, ExplodingHandler>());
        TestBoard board = await app.CreateBoardAsync();

        Result updated = await app.SendAsync(board.OwnerId, new UpdateBoardCommand(board.BoardId, "Renamed", ""));

        await using AppDbContext db = app.CreateDbContext();
        Assert.Equal("Renamed", await db.Boards.Where(b => b.Id == board.BoardId).Select(b => b.Title).SingleAsync());
        Assert.True(updated.IsSuccess, updated.IsFailure ? updated.Error.Code : null);
    }

    private sealed class ExplodingHandler : IDomainEventHandler<BoardUpdatedDomainEvent>
    {
        public Task Handle(BoardUpdatedDomainEvent domainEvent, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("side effect failed");
    }
}
