using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;
using static Snapflow.Application.Swimlanes.GetById.GetSwimlaneByIdResponse;

namespace Snapflow.Application.Swimlanes.GetById;

internal sealed class GetSwimlaneByIdHandler(
    IAppDbContext dbContext,
    IBoardMembershipService membershipService) : IQueryHandler<GetSwimlaneByIdQuery, GetSwimlaneByIdResponse>
{
    public async Task<Result<GetSwimlaneByIdResponse>> Handle(GetSwimlaneByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        bool isMember = await membershipService.IsMemberAsync(query.BoardId, cancellationToken);

        GetSwimlaneByIdResponse? swimlane = await dbContext.Swimlanes
            .AsNoTracking()
            .Where(s => !s.IsDeleted && s.Id == query.Id && s.BoardId == query.BoardId)
            .Select(s => new GetSwimlaneByIdResponse(
                s.Id,
                s.BoardId,
                s.Title,
                s.Rank,
                s.Height,
                s.CreatedAt,
                isMember ? UserDto.From(s.CreatedBy) : null,
                s.UpdatedAt,
                isMember ? UserDto.From(s.UpdatedBy) : null))
            .SingleOrDefaultAsync(cancellationToken);
        if (swimlane == null)
            return Result.Failure<GetSwimlaneByIdResponse>(SwimlaneErrors.NotFound(query.Id));
        return swimlane;
    }
}