using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Members.Add;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Members;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Boards;

internal sealed class AddMember : IEndpoint
{
    public sealed record AddMemberRequest(int UserId, MemberRole Role);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("boards/{boardId:int}/members", async (
            AddMemberRequest request,
            int boardId,
            ICommandHandler<AddMemberCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new AddMemberCommand(request.UserId, boardId, request.Role);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, Results.Problem);
        })
        .RequireAuthorization(BoardPermissions.Boards.Update)
        .WithTags(EndpointTags.Boards)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
