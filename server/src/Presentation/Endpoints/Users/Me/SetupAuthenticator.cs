using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Users.Me.TwoFactor.SetupAuthenticator;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Users.Me;

internal sealed class SetupAuthenticator : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("me/two-factor/authenticator", async (
            ICommandHandler<SetupAuthenticatorCommand, SetupAuthenticatorResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new SetupAuthenticatorCommand(), cancellationToken);

            return result.Match(Results.Ok, Results.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Users)
        .Produces<SetupAuthenticatorResponse>()
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
