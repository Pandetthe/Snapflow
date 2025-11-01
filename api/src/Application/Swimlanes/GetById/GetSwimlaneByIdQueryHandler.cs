using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;

namespace Snapflow.Application.Swimlanes.GetById;

internal sealed class GetSwimlaneByIdQueryHandler(
    IAppDbContext dbContext) : IQueryHandler<GetSwimlaneByIdQuery, GetSwimlaneByIdResponse>
{
    public async Task<Result<GetSwimlaneByIdResponse>> Handle(GetSwimlaneByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var swimlane = await dbContext.Swimlanes
            .AsNoTracking()
            .Where(s => !s.IsDeleted && s.Id == query.Id)
            .Select(s => new GetSwimlaneByIdResponse(s.Id, s.BoardId, s.Title))
            .SingleOrDefaultAsync(cancellationToken);
        if (swimlane == null)
            return Result.Failure<GetSwimlaneByIdResponse>(SwimlaneErrors.NotFound(query.Id));
        return Result.Success(swimlane);
    }
}