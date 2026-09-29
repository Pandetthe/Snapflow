using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;

namespace Snapflow.Application.Members.Get;

internal sealed class GetMembersQueryHandler(
    IAppDbContext dbContext,
    IBoardMembershipService membershipService) : IQueryHandler<GetMembersQuery, List<GetMembersResponse>>
{
    public async Task<Result<List<GetMembersResponse>>> Handle(GetMembersQuery query,
        CancellationToken cancellationToken = default)
    {
        bool boardExists = await dbContext.Boards
            .AsNoTracking()
            .AnyAsync(b => b.Id == query.BoardId && !b.IsDeleted, cancellationToken);
        if (!boardExists)
            return Result.Failure<List<GetMembersResponse>>(BoardErrors.NotFound(query.BoardId));

        if (!await membershipService.IsMemberAsync(query.BoardId, cancellationToken))
            return Result.Success(new List<GetMembersResponse>());

        var members = await dbContext.Members
            .AsNoTracking()
            .Where(b => b.BoardId == query.BoardId)
            .Select(b => new GetMembersResponse(b.UserId, b.User.UserName))
            .ToListAsync(cancellationToken);
        return Result.Success(members);
    }
}
