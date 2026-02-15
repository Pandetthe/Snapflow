using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Boards.Create;
using Snapflow.Common;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Boards;

internal sealed class Create : IEndpoint
{
    public sealed record CreateBoardRequest(string Title, string Description);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("boards", async (
            CreateBoardRequest request,
            ICommandHandler<CreateBoardCommand, int> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateBoardCommand(request.Title, request.Description);

            Result<int> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.OkWithId, Results.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Boards)
        .ProducesIdResponse()
        .ProducesCustomValidationProblem();
    }
}
