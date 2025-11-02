using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;

namespace Snapflow.Application.Lists.GetByBoardId;

internal sealed class GetListsByBoardIdQueryHandler(
    IAppDbContext dbContext) : IQueryHandler<GetListsByBoardIdQuery, ListsResponse>
{
    public async Task<Result<ListsResponse>> Handle(GetListsByBoardIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var board = await dbContext.Boards
            .AsNoTracking()
            .Where(b => b.Id == query.Id && !b.IsDeleted)
            .Select(b => new
            {
                b.Id,
                Lists = new ListsResponse(
                    b.Lists
                    .Where(l => !l.IsDeleted)
                    .Select(l => new ListResponse(
                        l.Id, 
                        l.BoardId, 
                        l.SwimlaneId, 
                        l.Title)))
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (board == null)
            return Result.Failure<ListsResponse>(BoardErrors.NotFound(query.Id));

        return board.Lists;
    }
}