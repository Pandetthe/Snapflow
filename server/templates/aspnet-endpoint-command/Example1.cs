using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Presentation.Extensions;

namespace SnapflowAspNet;

internal sealed class Example1 : IEndpoint
{
    public sealed record Example1Request(string Name);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("boards/{boardId:int}/example1", async (
            Example1Request request,
            int boardId,
            ICommandHandler<Example1Command> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new Example1Command(boardId, request.Name);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, Results.Problem);
        })
        .RequireAuthorization(BoardPermissions.Boards.Update)
        .WithTags(EndpointTags.Boards)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesCustomValidationProblem()
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
