using Snapflow.Api.Extensions;
using Snapflow.Api.Infrastructure;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Lists.Move;
using Snapflow.Common;
using Snapflow.Domain.Boards;

namespace Snapflow.Api.Endpoints.Lists;

internal sealed class Move : IEndpoint
{
    public sealed record MoveListRequest(int SwimlaneId, int? BeforeId);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("boards/{boardId:int}/lists/{listId:int}/move", async (
            MoveListRequest request,
            int boardId, int listId,
            ICommandHandler<MoveListCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new MoveListCommand(listId, request.SwimlaneId, request.BeforeId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireAuthorization(BoardPermissions.Lists.Move)
        .WithTags(EndpointTags.Lists)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}