using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Users.Me.Passkeys.CreateOptions;
using Snapflow.Presentation.Contracts;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Users.Me;

internal sealed class CreatePasskeyOptions : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("me/passkeys/options", async (
            ICommandHandler<CreatePasskeyOptionsCommand, PasskeyChallenge> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new CreatePasskeyOptionsCommand(), cancellationToken);

            return result.Match(challenge => Results.Ok(PasskeyOptionsResponse.From(challenge)), Results.Problem);
        })
        .RequireAuthorization()
        .RequirePasswordAuthentication()
        .WithTags(EndpointTags.Users)
        .Produces<PasskeyOptionsResponse>();
    }
}
