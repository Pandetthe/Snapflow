using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Lists.Delete;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Lists;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("boards/{boardId:int}/lists/{listId:int}", async (
            int boardId, int listId,
            ICommandHandler<DeleteListCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteListCommand(listId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, Results.Problem);
        })
        .RequireAuthorization(BoardPermissions.Lists.Delete)
        .WithTags(EndpointTags.Lists)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}