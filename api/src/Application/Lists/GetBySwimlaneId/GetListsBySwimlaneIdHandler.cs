using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;
using static Snapflow.Application.Lists.GetBySwimlaneId.GetListsBySwimlaneIdResponse;

namespace Snapflow.Application.Lists.GetBySwimlaneId;

internal sealed class GetListsBySwimlaneIdHandler(
    IAppDbContext dbContext) : IQueryHandler<GetListsBySwimlaneIdQuery, IReadOnlyList<ListDto>>
{
    public async Task<Result<IReadOnlyList<ListDto>>> Handle(GetListsBySwimlaneIdQuery query, CancellationToken cancellationToken = default)
    {
        var swimlane = await dbContext.Swimlanes
            .AsNoTracking()
            .Where(s => s.Id == query.Id && !s.IsDeleted)
            .Select(s => new
            {
                s.Id,
                Lists = s.Lists
                    .Where(l => !l.IsDeleted)
                    .OrderBy(s => s.Rank)
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

        if (swimlane == null)
            return Result.Failure<IReadOnlyList<ListDto>>(SwimlaneErrors.NotFound(query.Id));
        return swimlane.Lists;
    }
}