using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Swimlanes.Delete;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Swimlanes;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("boards/{boardId:int}/swimlanes/{swimlaneId:int}", async (
            int boardId, int swimlaneId,
            ICommandHandler<DeleteSwimlaneCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteSwimlaneCommand(swimlaneId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, Results.Problem);
        })
        .RequireAuthorization(BoardPermissions.Swimlanes.Delete)
        .WithTags(EndpointTags.Swimlanes)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}