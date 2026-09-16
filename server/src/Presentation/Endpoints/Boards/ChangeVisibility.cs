using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Boards.ChangeVisibility;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Boards;

internal sealed class ChangeVisibility : IEndpoint
{
    public sealed record ChangeVisibilityRequest(BoardVisibility Visibility);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("boards/{boardId:int}/visibility", async (
            int boardId,
            ChangeVisibilityRequest request,
            ICommandHandler<ChangeBoardVisibilityCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ChangeBoardVisibilityCommand(boardId, request.Visibility);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, Results.Problem);
        })
        .RequireAuthorization(BoardPermissions.Boards.ChangeVisibility)
        .WithTags(EndpointTags.Boards)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
