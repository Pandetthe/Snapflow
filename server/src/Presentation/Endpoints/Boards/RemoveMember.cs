using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Members.Remove;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Boards;

internal sealed class RemoveMember : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("boards/{boardId:int}/members/{userId:int}", async (
            int boardId,
            int userId,
            ICommandHandler<RemoveMemberCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RemoveMemberCommand(boardId, userId);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, Results.Problem);
        })
        .RequireAuthorization(BoardPermissions.Boards.Update)
        .WithTags(EndpointTags.Boards)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
