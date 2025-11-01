using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;

namespace Snapflow.Application.Swimlanes.GetByBoardId;

internal sealed class GetSwimlaneByBoardIdQueryHandler(
    IAppDbContext dbContext) : IQueryHandler<GetSwimlaneByBoardIdQuery, List<GetSwimlaneByBoardIdResponse>>
{
    public async Task<Result<List<GetSwimlaneByBoardIdResponse>>> Handle(GetSwimlaneByBoardIdQuery query, 
        CancellationToken cancellationToken = default)
    {
        var board = await dbContext.Boards
            .Include(b => b.Swimlanes.Where(s => !s.IsDeleted))
            .AsNoTracking()
            .Select(b => new
            {
                b.Id,
                Swimlanes = b.Swimlanes
                .Select(s => new GetSwimlaneByBoardIdResponse(s.Id, s.BoardId, s.Title))
                .ToList()
            })
            .SingleOrDefaultAsync(b => b.Id == query.Id, cancellationToken);
        if (board == null)
            return Result.Failure<List<GetSwimlaneByBoardIdResponse>>(BoardErrors.NotFound(query.Id));
        return Result.Success(board.Swimlanes);
    }
}