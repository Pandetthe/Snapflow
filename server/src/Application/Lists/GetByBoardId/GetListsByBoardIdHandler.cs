using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using static Snapflow.Application.Lists.GetByBoardId.GetListsByBoardId;

namespace Snapflow.Application.Lists.GetByBoardId;

internal sealed class GetListsByBoardIdHandler(
    IAppDbContext dbContext) : IQueryHandler<GetListsByBoardIdQuery, IReadOnlyList<ListDto>>
{
    public async Task<Result<IReadOnlyList<ListDto>>> Handle(GetListsByBoardIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var board = await dbContext.Boards
            .AsNoTracking()
            .Where(b => b.Id == query.Id && !b.IsDeleted)
            .Select(b => new
            {
                b.Id,
                Lists = b.Lists
                .Where(l => !l.IsDeleted)
                .OrderBy(l => l.Rank)
                .Select(l => new ListDto(
                    l.Id,
                    l.BoardId,
                    l.SwimlaneId,
                    l.Title,
                    l.Rank,
                    l.Width))
                .ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (board == null)
            return Result.Failure<IReadOnlyList<ListDto>>(BoardErrors.NotFound(query.Id));

        return board.Lists;
    }
}