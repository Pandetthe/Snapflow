using Snapflow.Api.Extensions;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Boards.Create;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Boards;

internal sealed class Create : IEndpoint
{
    public sealed class Request
    {
        public required string Title { get; init; }

        public string Description { get; init; } = "";
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("boards", async (
            Request request,
            ICommandHandler<CreateBoardCommand, int> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateBoardCommand
            { 
                Title = request.Title, 
                Description = request.Description 
            };

            Result<int> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Boards)
        .RequireAuthorization();
    }
}
