using Snapflow.Api.Extensions;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Lists.Create;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Lists;

internal sealed class Create : IEndpoint
{
    public sealed record Request(string Title);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("boards/{boardId}/swimlanes/{swimlaneId}/lists", async (
            Request request,
            int boardId,
            int swimlaneId,
            ICommandHandler<CreateListCommand, int> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateListCommand(boardId, swimlaneId, request.Title);

            Result<int> result = await handler.Handle(command, cancellationToken);

            return result.Match(CustomResults.OkWithId, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Lists);
    }
}