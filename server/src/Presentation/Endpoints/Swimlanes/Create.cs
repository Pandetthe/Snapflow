using Microsoft.AspNetCore.Mvc;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Swimlanes.Create;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Swimlanes;

internal sealed class Create : IEndpoint
{
    public sealed record CreateSwimlaneRequest(string Title, int? Height, int? BeforeId);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("boards/{boardId:int}/swimlanes", async (
            [FromBody] CreateSwimlaneRequest request,
            [FromRoute] int boardId,
            [FromServices] ICommandHandler<CreateSwimlaneCommand, CreateSwimlaneResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateSwimlaneCommand(boardId, request.Title, request.Height, request.BeforeId);

            Result<CreateSwimlaneResponse> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, Results.Problem);
        })
        .RequireAuthorization(BoardPermissions.Swimlanes.Create)
        .WithTags(EndpointTags.Swimlanes)
        .Produces<CreateSwimlaneResponse>(StatusCodes.Status200OK)
        .ProducesCustomValidationProblem();
    }
}