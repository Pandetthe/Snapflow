using Snapflow.Api.Extensions;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Boards.Delete;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Boards;

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

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireAuthorization("Board:Delete")
        .WithTags(EndpointTags.Boards); 
    }
}
