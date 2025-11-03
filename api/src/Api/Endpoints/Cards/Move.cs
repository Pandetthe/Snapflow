using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Swimlanes.Move;

namespace Snapflow.Api.Endpoints.Cards;

internal sealed class Move : IEndpoint
{
    public sealed record MoveCardRequest();

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("boards/{boardId:int}/cards/{cardId:int}/move", (
            MoveCardRequest request,
            int boardId, int cardId,
            ICommandHandler<MoveSwimlaneCommand> handler,
            CancellationToken cancellationToken) =>
        {
            throw new NotImplementedException();
            // TODO
            //var command = new MoveSwimlaneCommand();

            //Result result = await handler.Handle(command, cancellationToken);

            //return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Cards);
    }
}