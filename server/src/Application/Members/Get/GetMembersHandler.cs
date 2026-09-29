using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;

namespace Snapflow.Application.Members.Get;

internal sealed class GetMembersHandler(
    IAppDbContext dbContext,
    IBoardMembershipService membershipService) : IQueryHandler<GetMembersQuery, IReadOnlyList<GetMembersResponse>>
{
    public async Task<Result<IReadOnlyList<GetMembersResponse>>> Handle(GetMembersQuery query,
        CancellationToken cancellationToken = default)
    {
        bool boardExists = await dbContext.Boards
            .AsNoTracking()
            .AnyAsync(b => b.Id == query.BoardId, cancellationToken);
        if (!boardExists)
            return BoardErrors.NotFound(query.BoardId);

        if (!await membershipService.IsMemberAsync(query.BoardId, cancellationToken))
            return Result.Success<IReadOnlyList<GetMembersResponse>>([]);

        var members = await dbContext.Members
            .AsNoTracking()
            .Where(b => b.BoardId == query.BoardId)
            .Select(b => new GetMembersResponse(b.UserId, b.User.UserName))
            .ToListAsync(cancellationToken);
        return members;
    }
}
