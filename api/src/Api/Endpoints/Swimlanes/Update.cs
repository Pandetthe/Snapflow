using Snapflow.Api.Extensions;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Swimlanes.Update;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Swimlanes;

internal sealed class Update : IEndpoint
{
    public sealed record UpdateSwimlaneRequest(string Title);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("boards/{boardId:int}/swimlanes/{swimlaneId:int}", async (
            UpdateSwimlaneRequest request,
            int boardId, int swimlaneId,
            ICommandHandler<UpdateSwimlaneCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateSwimlaneCommand(swimlaneId, request.Title);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Swimlanes);
    }
}