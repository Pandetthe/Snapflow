using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Boards.Update;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Boards;

internal sealed class Update : IEndpoint
{
    public sealed record UpdateBoardRequest(
        string Title,
        string Description,
        IReadOnlyList<UpdateBoardMemberRequest>? Members = null,
        BoardVisibility? Visibility = null);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("boards/{boardId:int}", async (
            UpdateBoardRequest request,
            int boardId,
            ICommandHandler<UpdateBoardCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateBoardCommand(
                boardId,
                request.Title,
                request.Description,
                request.Members,
                request.Visibility);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, Results.Problem);
        })
        .RequireAuthorization(BoardPermissions.Boards.Update)
        .WithTags(EndpointTags.Boards)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}