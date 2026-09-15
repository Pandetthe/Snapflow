using Microsoft.AspNetCore.Mvc;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Tags.Create;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Tags;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Tags;

internal sealed class Create : IEndpoint
{
    public sealed record CreateTagRequest(string Title, TagColors Color);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("boards/{boardId:int}/tags", async (
            [FromBody] CreateTagRequest request,
            [FromRoute] int boardId,
            [FromServices] ICommandHandler<CreateTagCommand, CreateTagResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateTagCommand(boardId, request.Title, request.Color);

            Result<CreateTagResponse> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, Results.Problem);
        })
        .RequireAuthorization(BoardPermissions.Tags.Create)
        .WithTags(EndpointTags.Tags)
        .Produces<CreateTagResponse>(StatusCodes.Status200OK)
        .ProducesCustomValidationProblem();
    }
}
