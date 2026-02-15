using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Auth.SignOut;
using Snapflow.Common;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Auth;

internal sealed class SignOut : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/sign-out", async (
            ICommandHandler<SignOutCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new SignOutCommand();

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, Results.Problem);
        })
        .WithTags(EndpointTags.Auth)
        .RequireAuthorization();
    }
}