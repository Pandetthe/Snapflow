using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;

namespace Snapflow.Application.BoardMembers.Get;

internal sealed class GetBoardMembersQueryHandler(
    IAppDbContext dbContext) : IQueryHandler<GetBoardMembersQuery, List<GetBoardMembersResponse>>
{
    public async Task<Result<List<GetBoardMembersResponse>>> Handle(GetBoardMembersQuery query, 
        CancellationToken cancellationToken = default)
    {
        var members = await dbContext.BoardMembers
            .Where(b => b.BoardId == query.BoardId)
            .Include(b => b.User)
            .Select(b => new GetBoardMembersResponse(b.UserId, b.User.UserName))
            .ToListAsync(cancellationToken);
        if (members.Count == 0)
            return Result.Failure<List<GetBoardMembersResponse>>(BoardErrors.NotFound(query.BoardId));
        return Result.Success(members);
    }
}