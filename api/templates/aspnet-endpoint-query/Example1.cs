using Snapflow.Api.Extensions;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;

namespace SnapflowAspNet;

internal sealed class Example1 : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("Example1", async (
            IQueryHandler<Example1Query, Example1Response> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new Example1Query();

            Result<Example1Response> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization();
    }
}