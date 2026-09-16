using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;

namespace Snapflow.Application.Boards.GetVisibilityOptions;

internal sealed class GetBoardVisibilityOptionsHandler(
    IBoardVisibilityPolicy visibilityPolicy) : IQueryHandler<GetBoardVisibilityOptionsQuery, GetBoardVisibilityOptionsResponse>
{
    public Task<Result<GetBoardVisibilityOptionsResponse>> Handle(GetBoardVisibilityOptionsQuery query, CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Success(new GetBoardVisibilityOptionsResponse(visibilityPolicy.AllowedVisibilities)));
}
