using Snapflow.Api.Extensions;
using Snapflow.Api.Infrastructure;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Swimlanes.Create;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Swimlanes;

internal sealed class Create : IEndpoint
{
    public sealed record CreateSwimlaneRequest(string Title, int? BeforeId);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("boards/{boardId:int}/swimlanes", async (
            CreateSwimlaneRequest request,
            int boardId,
            ICommandHandler<CreateSwimlaneCommand, int> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateSwimlaneCommand(boardId, request.Title, request.BeforeId);

            Result<int> result = await handler.Handle(command, cancellationToken);

            return result.Match(CustomResults.OkWithId, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Swimlanes);
    }
}