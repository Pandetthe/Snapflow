using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Lists;
using static Snapflow.Application.Lists.GetById.GetListByIdResponse;

namespace Snapflow.Application.Lists.GetById;

internal sealed class GetListByIdHandler(
    IAppDbContext dbContext) : IQueryHandler<GetListByIdQuery, GetListByIdResponse>
{
    public async Task<Result<GetListByIdResponse>> Handle(GetListByIdQuery query, CancellationToken cancellationToken = default)
    {
        var list = await dbContext.Lists
            .AsNoTracking()
            .Where(l => l.Id == query.Id && !l.IsDeleted)
            .Select(l => new GetListByIdResponse(
                l.Id,
                l.BoardId,
                l.SwimlaneId,
                l.Title,
                l.Rank,
                l.Width,
                l.CreatedAt,
                UserDto.From(l.CreatedBy),
                l.UpdatedAt,
                UserDto.From(l.UpdatedBy)))
            .SingleOrDefaultAsync(cancellationToken);
        if (list == null)
            return Result.Failure<GetListByIdResponse>(ListErrors.NotFound(query.Id));
        return list;
    }
}