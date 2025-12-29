using Snapflow.Api.Extensions;
using Snapflow.Api.Infrastructure;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Lists.Create;
using Snapflow.Common;
using Snapflow.Domain.Boards;

namespace Snapflow.Api.Endpoints.Lists;

internal sealed class Create : IEndpoint
{
    public sealed record CreateListRequest(string Title, int? Width, int? BeforeId);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("boards/{boardId:int}/swimlanes/{swimlaneId:int}/lists", async (
            CreateListRequest request,
            int boardId,
            int swimlaneId,
            ICommandHandler<CreateListCommand, int> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateListCommand(swimlaneId, request.Title, request.Width, request.BeforeId);

            Result<int> result = await handler.Handle(command, cancellationToken);

            return result.Match(CustomResults.OkWithId, CustomResults.Problem);
        })
        .RequireAuthorization(BoardPermissions.Lists.Create)
        .WithTags(EndpointTags.Lists)
        .ProducesIdResponse()
        .ProducesCustomValidationProblem();
    }
}