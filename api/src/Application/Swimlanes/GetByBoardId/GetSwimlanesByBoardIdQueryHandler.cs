using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;

namespace Snapflow.Application.Swimlanes.GetByBoardId;

internal sealed class GetSwimlanesByBoardIdQueryHandler(
    IAppDbContext dbContext) : IQueryHandler<GetSwimlanesByBoardIdQuery, SwimlanesResponse>
{
    public async Task<Result<SwimlanesResponse>> Handle(GetSwimlanesByBoardIdQuery query, 
        CancellationToken cancellationToken = default)
    {
        var board = await dbContext.Boards
            .AsNoTracking()
            .Where(b => b.Id == query.Id && !b.IsDeleted)
            .Select(b => new
            {
                b.Id,
                Swimlanes = new SwimlanesResponse(
                    b.Swimlanes
                    .Where(s => !s.IsDeleted)
                    .Select(s => new SwimlaneResponse(
                        s.Id, 
                        s.BoardId,
                        s.Title)))
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (board == null)
            return Result.Failure<SwimlanesResponse>(BoardErrors.NotFound(query.Id));
        return board.Swimlanes;
    }
}