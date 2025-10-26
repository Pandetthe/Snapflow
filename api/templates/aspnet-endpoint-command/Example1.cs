using Snapflow.Api.Extensions;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;

namespace SnapflowAspNet;

internal sealed class Example1 : IEndpoint
{
    public sealed record Request(int Id);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("Example1", async (
            Request request,
            ICommandHandler<Example1Command, int> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new Example1Command(request.Id);

            Result<int> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization();
    }
}