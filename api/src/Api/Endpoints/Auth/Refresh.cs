using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Auth.Refresh;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Auth;

internal sealed class Refresh : IEndpoint
{
    public sealed class RefreshRequest
    {
        public required string RefreshToken { get; init; }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/refresh", async (
            RefreshRequest request,
            ICommandHandler<RefreshCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RefreshCommand(request.RefreshToken);

            Result result = await handler.Handle(command, cancellationToken);
            if (result.IsFailure)
                return CustomResults.Problem(result);
            return Results.Empty; // Application returns data via asp.net authentication mechanisms
        })
            .WithTags(EndpointTags.Auth);
    }
}