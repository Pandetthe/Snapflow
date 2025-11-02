using Snapflow.Api.Extensions;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Boards.Update;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Boards;

internal sealed class Update : IEndpoint
{
    public sealed record UpdateBoardRequest(string Title, string Description);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("boards/{boardId:int}", async (
            UpdateBoardRequest request,
            int boardId,
            ICommandHandler<UpdateBoardCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateBoardCommand(boardId, request.Title, request.Description);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Boards);
    }
}