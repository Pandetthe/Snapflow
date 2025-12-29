using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;
using static Snapflow.Application.Swimlanes.GetById.GetSwimlaneByIdResponse;

namespace Snapflow.Application.Swimlanes.GetById;

internal sealed class GetSwimlaneByIdHandler(
    IAppDbContext dbContext) : IQueryHandler<GetSwimlaneByIdQuery, GetSwimlaneByIdResponse>
{
    public async Task<Result<GetSwimlaneByIdResponse>> Handle(GetSwimlaneByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var swimlane = await dbContext.Swimlanes
            .AsNoTracking()
            .Where(s => !s.IsDeleted && s.Id == query.Id)
            .Select(s => new GetSwimlaneByIdResponse(
                s.Id,
                s.BoardId,
                s.Title,
                s.Rank,
                s.Height,
                s.CreatedAt,
                UserDto.From(s.CreatedBy),
                s.UpdatedAt,
                UserDto.From(s.UpdatedBy)))
            .SingleOrDefaultAsync(cancellationToken);
        if (swimlane == null)
            return Result.Failure<GetSwimlaneByIdResponse>(SwimlaneErrors.NotFound(query.Id));
        return swimlane;
    }
}