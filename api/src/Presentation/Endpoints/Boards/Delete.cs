using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Boards.Delete;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Boards;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("boards/{boardId:int}", async (
            int boardId,
            ICommandHandler<DeleteBoardCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteBoardCommand(boardId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, Results.Problem);
        })
        .RequireAuthorization(BoardPermissions.Boards.Delete)
        .WithTags(EndpointTags.Boards)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
