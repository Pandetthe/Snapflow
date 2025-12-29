using Snapflow.Api.Extensions;
using Snapflow.Api.Infrastructure;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Lists.Update;
using Snapflow.Common;
using Snapflow.Domain.Boards;

namespace Snapflow.Api.Endpoints.Lists;

internal sealed class Update : IEndpoint
{
    public sealed record UpdateListRequest(string Title, int? Width);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("boards/{boardId:int}/lists/{listId:int}", async (
            UpdateListRequest request,
            int boardId, int listId,
            ICommandHandler<UpdateListCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateListCommand(listId, request.Title, request.Width);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireAuthorization(BoardPermissions.Lists.Update)
        .WithTags(EndpointTags.Lists)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}