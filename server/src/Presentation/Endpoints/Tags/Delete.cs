using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Tags.Delete;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Tags;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("boards/{boardId:int}/tags/{tagId:int}", async (
            int boardId, int tagId,
            ICommandHandler<DeleteTagCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DeleteTagCommand(boardId, tagId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, Results.Problem);
        })
        .RequireAuthorization(BoardPermissions.Tags.Delete)
        .WithTags(EndpointTags.Tags)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
