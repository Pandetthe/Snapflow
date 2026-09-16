using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Lists;
using static Snapflow.Application.Lists.GetById.GetListByIdResponse;

namespace Snapflow.Application.Lists.GetById;

internal sealed class GetListByIdHandler(
    IAppDbContext dbContext,
    IBoardMembershipService membershipService) : IQueryHandler<GetListByIdQuery, GetListByIdResponse>
{
    public async Task<Result<GetListByIdResponse>> Handle(GetListByIdQuery query, CancellationToken cancellationToken = default)
    {
        bool isMember = await membershipService.IsMemberAsync(query.BoardId, cancellationToken);

        GetListByIdResponse? list = await dbContext.Lists
            .AsNoTracking()
            .Where(l => l.Id == query.Id && l.BoardId == query.BoardId && !l.IsDeleted)
            .Select(l => new GetListByIdResponse(
                l.Id,
                l.BoardId,
                l.SwimlaneId,
                l.Title,
                l.Rank,
                l.Width,
                l.CreatedAt,
                isMember ? UserDto.From(l.CreatedBy) : null,
                l.UpdatedAt,
                isMember ? UserDto.From(l.UpdatedBy) : null))
            .SingleOrDefaultAsync(cancellationToken);
        if (list == null)
            return Result.Failure<GetListByIdResponse>(ListErrors.NotFound(query.Id));
        return list;
    }
}