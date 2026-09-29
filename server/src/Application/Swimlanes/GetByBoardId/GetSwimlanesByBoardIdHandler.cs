using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using static Snapflow.Application.Swimlanes.GetByBoardId.GetSwimlanesByBoardIdResponse;

namespace Snapflow.Application.Swimlanes.GetByBoardId;

internal sealed class GetSwimlanesByBoardIdHandler(
    IAppDbContext dbContext) : IQueryHandler<GetSwimlanesByBoardIdQuery, IReadOnlyList<SwimlaneDto>>
{
    public async Task<Result<IReadOnlyList<SwimlaneDto>>> Handle(GetSwimlanesByBoardIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var board = await dbContext.Boards
            .AsNoTracking()
            .Where(b => b.Id == query.Id)
            .Select(b => new
            {
                b.Id,
                Swimlanes = b.Swimlanes
                    .OrderBy(s => s.Rank)
                    .Select(s => new SwimlaneDto(
                        s.Id,
                        s.Title,
                        s.Rank,
                        s.Height))
                    .ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (board == null)
            return BoardErrors.NotFound(query.Id);
        return board.Swimlanes;
    }
}