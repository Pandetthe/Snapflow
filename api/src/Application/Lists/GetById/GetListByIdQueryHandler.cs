using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Lists;

namespace Snapflow.Application.Lists.GetById;

internal sealed class GetListByIdQueryHandler(
    IAppDbContext dbContext) : IQueryHandler<GetListByIdQuery, ListResponse>
{
    public async Task<Result<ListResponse>> Handle(GetListByIdQuery query, CancellationToken cancellationToken = default)
    {
        var list = await dbContext.Lists
            .AsNoTracking()
            .Where(l => l.Id == query.Id && !l.IsDeleted)
            .Select(l => new ListResponse(
                l.Id,
                l.BoardId,
                l.SwimlaneId,
                l.Title,
                l.CreatedAt,
                new UserResponse(l.CreatedBy.Id, l.CreatedBy.UserName),
                l.UpdatedAt,
                l.UpdatedBy == null ? null : new UserResponse(l.UpdatedBy.Id, l.UpdatedBy.UserName)))
            .SingleOrDefaultAsync(cancellationToken);
        if (list == null)
            return Result.Failure<ListResponse>(ListErrors.NotFound(query.Id));
        return list;
    }
}