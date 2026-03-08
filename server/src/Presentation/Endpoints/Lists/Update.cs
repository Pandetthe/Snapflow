using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Lists.Update;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Lists;

internal sealed class Update : IEndpoint
{
    public sealed record UpdateListRequest(string Title, int? Width);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("boards/{boardId:int}/lists/{listId:int}", async (
            UpdateListRequest request,
            int boardId, int listId,
            ICommandHandler<UpdateListCommand, UpdateListResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateListCommand(listId, request.Title, request.Width);

            Result<UpdateListResponse> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, Results.Problem);
        })
        .RequireAuthorization(BoardPermissions.Lists.Update)
        .WithTags(EndpointTags.Lists)
        .Produces<UpdateListResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}