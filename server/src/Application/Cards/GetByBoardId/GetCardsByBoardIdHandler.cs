using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using static Snapflow.Application.Cards.GetByBoardId.GetCardsByBoardIdResponse;

namespace Snapflow.Application.Cards.GetByBoardId;

internal sealed class GetCardsByBoardIdHandler(
    IAppDbContext dbContext) : IQueryHandler<GetCardsByBoardIdQuery, IReadOnlyList<CardDto>>
{
    public async Task<Result<IReadOnlyList<CardDto>>> Handle(GetCardsByBoardIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var board = await dbContext.Boards
            .AsNoTracking()
            .Where(b => b.Id == query.Id && !b.IsDeleted)
            .Select(b => new
            {
                b.Id,
                Cards = b.Cards
                    .Where(c => !c.IsDeleted)
                    .OrderBy(c => c.Rank)
                    .Select(c => new CardDto(
                        c.Id,
                        c.BoardId,
                        c.SwimlaneId,
                        c.ListId,
                        c.Title,
                        c.Description,
                        c.Rank))
                    .ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (board == null)
            return Result.Failure<IReadOnlyList<CardDto>>(BoardErrors.NotFound(query.Id));

        return board.Cards;
    }
}