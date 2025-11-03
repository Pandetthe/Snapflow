using Snapflow.Api.Extensions;
using Snapflow.Api.Infrastructure;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Boards.Create;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Boards;

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

            return result.Match(CustomResults.OkWithId, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Boards)
        .ProducesIdResponse()
        .ProducesCustomValidationProblem()
        .ProducesInternalServerError();
    }
}
