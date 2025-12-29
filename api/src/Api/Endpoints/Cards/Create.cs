using Snapflow.Api.Extensions;
using Snapflow.Api.Infrastructure;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Cards.Create;
using Snapflow.Common;
using Snapflow.Domain.Boards;

namespace Snapflow.Api.Endpoints.Cards;

internal sealed class Create : IEndpoint
{
    public sealed record CreateCardRequest(string Title, string Description, int? BeforeId);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("boards/{boardId:int}/lists/{listId:int}/cards", async (
            CreateCardRequest request,
            int boardId,
            int listId,
            ICommandHandler<CreateCardCommand, int> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateCardCommand(listId, request.Title, request.Description, request.BeforeId);

            Result<int> result = await handler.Handle(command, cancellationToken);

            return result.Match(CustomResults.OkWithId, CustomResults.Problem);
        })
        .RequireAuthorization(BoardPermissions.Cards.Create)
        .WithTags(EndpointTags.Cards)
        .ProducesIdResponse()
        .ProducesCustomValidationProblem();
    }
}