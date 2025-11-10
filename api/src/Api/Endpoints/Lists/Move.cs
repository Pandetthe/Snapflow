using Snapflow.Api.Extensions;
using Snapflow.Api.Infrastructure;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Lists.Move;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Lists;

internal sealed class Move : IEndpoint
{
    public sealed record MoveListRequest(int? BeforeId);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("boards/{boardId:int}/lists/{listId:int}/move", async (
            MoveListRequest request,
            int boardId, int listId,
            ICommandHandler<MoveListCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new MoveListCommand(listId, request.BeforeId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Lists);
    }
}