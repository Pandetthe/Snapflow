using Microsoft.AspNetCore.Mvc;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Tags.Update;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Tags;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Tags;

internal sealed class Update : IEndpoint
{
    public sealed record UpdateTagRequest(string Title, TagColors Color);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("boards/{boardId:int}/tags/{tagId:int}", async (
            [FromBody] UpdateTagRequest request,
            [FromRoute] int boardId,
            [FromRoute] int tagId,
            [FromServices] ICommandHandler<UpdateTagCommand, UpdateTagResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateTagCommand(boardId, tagId, request.Title, request.Color);

            Result<UpdateTagResponse> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, Results.Problem);
        })
        .RequireAuthorization(BoardPermissions.Tags.Update)
        .WithTags(EndpointTags.Tags)
        .Produces<UpdateTagResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesCustomValidationProblem();
    }
}
