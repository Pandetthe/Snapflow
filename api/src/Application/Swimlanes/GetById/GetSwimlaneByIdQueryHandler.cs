using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;

namespace Snapflow.Application.Swimlanes.GetById;

internal sealed class GetSwimlaneByIdQueryHandler(
    IAppDbContext dbContext) : IQueryHandler<GetSwimlaneByIdQuery, SwimlaneResponse>
{
    public async Task<Result<SwimlaneResponse>> Handle(GetSwimlaneByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var swimlane = await dbContext.Swimlanes
            .AsNoTracking()
            .Where(s => !s.IsDeleted && s.Id == query.Id)
            .Select(s => new SwimlaneResponse(
                s.Id,
                s.BoardId,
                s.Title,
                s.CreatedAt,
                new UserResponse(s.CreatedBy.Id, s.CreatedBy.UserName),
                s.UpdatedAt,
                s.UpdatedBy == null ? null : new UserResponse(s.UpdatedBy.Id, s.UpdatedBy.UserName)))
            .SingleOrDefaultAsync(cancellationToken);
        if (swimlane == null)
            return Result.Failure<SwimlaneResponse>(SwimlaneErrors.NotFound(query.Id));
        return swimlane;
    }
}