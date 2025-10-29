using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;

namespace Snapflow.Application.BoardMembers.Get;

internal sealed class GetBoardMembersQueryHandler : IQueryHandler<GetBoardMembersQuery, GetBoardMembersResponse>
{
    public Task<Result<GetBoardMembersResponse>> Handle(GetBoardMembersQuery query, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}