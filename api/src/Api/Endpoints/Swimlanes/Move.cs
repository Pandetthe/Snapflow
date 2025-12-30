using Snapflow.Api.Extensions;
using Snapflow.Api.Infrastructure;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Swimlanes.Move;
using Snapflow.Common;
using Snapflow.Domain.Boards;

namespace Snapflow.Api.Endpoints.Swimlanes;

internal sealed class Move : IEndpoint
{
    public sealed record MoveSwimlaneRequest(int? BeforeId);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("boards/{boardId:int}/swimlanes/{swimlaneId:int}/move", async (
            MoveSwimlaneRequest request,
            int boardId, int swimlaneId,
            ICommandHandler<MoveSwimlaneCommand, string> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new MoveSwimlaneCommand(swimlaneId, request.BeforeId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireAuthorization(BoardPermissions.Swimlanes.Move)
        .WithTags(EndpointTags.Swimlanes)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}