using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;

namespace Snapflow.Application.Cards.GetByBoardId;

internal sealed class GetCardsByBoardIdQueryHandler(
    IAppDbContext dbContext) : IQueryHandler<GetCardsByBoardIdQuery, CardsResponse>
{
    public async Task<Result<CardsResponse>> Handle(GetCardsByBoardIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var board = await dbContext.Boards
            .AsNoTracking()
            .Where(b => b.Id == query.Id && !b.IsDeleted)
            .Select(b => new
            {
                b.Id,
                Cards = new CardsResponse(
                    b.Cards
                    .Where(c => !c.IsDeleted)
                    .Select(c => new CardResponse(
                        c.Id,
                        c.BoardId,
                        c.SwimlaneId,
                        c.ListId,
                        c.Title,
                        c.Description)))
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (board == null)
            return Result.Failure<CardsResponse>(BoardErrors.NotFound(query.Id));

        return board.Cards;
    }
}