using Snapflow.Api.Extensions;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Swimlanes.Move;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Swimlanes;

internal sealed class Move : IEndpoint
{
    public sealed record MoveSwimlaneRequest();

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("boards/{boardId:int}/swimlanes/{swimlaneId:int}/move", (
            MoveSwimlaneRequest request,
            int boardId, int swimlaneId,
            ICommandHandler<MoveSwimlaneCommand> handler,
            CancellationToken cancellationToken) =>
        {
            throw new NotImplementedException();
            // TODO
            //var command = new MoveSwimlaneCommand();

            //Result result = await handler.Handle(command, cancellationToken);

            //return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Swimlanes);
    }
}