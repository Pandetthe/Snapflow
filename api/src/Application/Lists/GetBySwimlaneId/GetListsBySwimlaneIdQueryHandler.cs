using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;

namespace Snapflow.Application.Lists.GetBySwimlaneId;

internal sealed class GetListsBySwimlaneIdQueryHandler(
    IAppDbContext dbContext) : IQueryHandler<GetListsBySwimlaneIdQuery, ListsResponse>
{
    public async Task<Result<ListsResponse>> Handle(GetListsBySwimlaneIdQuery query, CancellationToken cancellationToken = default)
    {
        var swimlane = await dbContext.Swimlanes
            .AsNoTracking()
            .Where(s => s.Id == query.Id && !s.IsDeleted)
            .Select(s => new
            {
                s.Id,
                Lists = new ListsResponse(
                    s.Lists
                    .Where(l => !l.IsDeleted)
                    .Select(l => new ListResponse(
                        l.Id,
                        l.BoardId,
                        l.SwimlaneId,
                        l.Title)))
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (swimlane == null)
            return Result.Failure<ListsResponse>(SwimlaneErrors.NotFound(query.Id));
        return swimlane.Lists;
    }
}