using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Members.ChangeOwner;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Boards;

internal sealed class ChangeOwner : IEndpoint
{
    public sealed record ChangeOwnerRequest(int UserId);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("boards/{boardId:int}/change-owner", async (
            int boardId,
            ChangeOwnerRequest request,
            ICommandHandler<ChangeOwnerCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ChangeOwnerCommand(request.UserId, boardId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, Results.Problem);
        })
        .RequireAuthorization(BoardPermissions.Boards.Transfer)
        .WithTags(EndpointTags.Boards)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
