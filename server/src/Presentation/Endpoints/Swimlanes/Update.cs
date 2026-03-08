using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Swimlanes.Update;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Swimlanes;

internal sealed class Update : IEndpoint
{
    public sealed record UpdateSwimlaneRequest(string Title, int? Height);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("boards/{boardId:int}/swimlanes/{swimlaneId:int}", async (
            UpdateSwimlaneRequest request,
            int boardId, int swimlaneId,
            ICommandHandler<UpdateSwimlaneCommand, UpdateSwimlaneResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateSwimlaneCommand(swimlaneId, request.Title, request.Height);

            Result<UpdateSwimlaneResponse> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, Results.Problem);
        })
        .RequireAuthorization(BoardPermissions.Swimlanes.Update)
        .WithTags(EndpointTags.Swimlanes)
        .Produces<UpdateSwimlaneResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}