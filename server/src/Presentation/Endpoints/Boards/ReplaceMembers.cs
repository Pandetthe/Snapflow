using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Members.Replace;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Members;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Boards;

internal sealed class ReplaceMembers : IEndpoint
{
    public sealed record ReplaceMembersRequest(IReadOnlyList<ReplaceMemberRequest> Members);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("boards/{boardId:int}/members", async (
            int boardId,
            ReplaceMembersRequest request,
            ICommandHandler<ReplaceMembersCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ReplaceMembersCommand(boardId, request.Members);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, Results.Problem);
        })
        .RequireAuthorization(BoardPermissions.Boards.Update)
        .WithTags(EndpointTags.Boards)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesCustomValidationProblem();
    }
}
