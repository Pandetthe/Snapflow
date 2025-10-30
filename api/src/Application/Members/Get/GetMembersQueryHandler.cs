using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;

namespace Snapflow.Application.Members.Get;

internal sealed class GetMembersQueryHandler(
    IAppDbContext dbContext) : IQueryHandler<GetMembersQuery, List<GetMembersResponse>>
{
    public async Task<Result<List<GetMembersResponse>>> Handle(GetMembersQuery query, 
        CancellationToken cancellationToken = default)
    {
        var members = await dbContext.Members
            .Where(b => b.BoardId == query.BoardId)
            .Include(b => b.User)
            .Select(b => new GetMembersResponse(b.UserId, b.User.UserName))
            .ToListAsync(cancellationToken);
        if (members.Count == 0)
            return Result.Failure<List<GetMembersResponse>>(BoardErrors.NotFound(query.BoardId));
        return Result.Success(members);
    }
}