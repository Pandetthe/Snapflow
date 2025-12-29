using Snapflow.Api.Extensions;
using Snapflow.Api.Infrastructure;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Swimlanes.Update;
using Snapflow.Common;
using Snapflow.Domain.Boards;

namespace Snapflow.Api.Endpoints.Swimlanes;

internal sealed class Update : IEndpoint
{
    public sealed record UpdateSwimlaneRequest(string Title, int? Height);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("boards/{boardId:int}/swimlanes/{swimlaneId:int}", async (
            UpdateSwimlaneRequest request,
            int boardId, int swimlaneId,
            ICommandHandler<UpdateSwimlaneCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateSwimlaneCommand(swimlaneId, request.Title, request.Height);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireAuthorization(BoardPermissions.Swimlanes.Update)
        .WithTags(EndpointTags.Swimlanes)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}