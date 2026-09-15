using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Users.Me.Passkeys.Add;
using Snapflow.Presentation.Contracts;
using Snapflow.Presentation.Extensions;
using System.Text.Json;

namespace Snapflow.Presentation.Endpoints.Users.Me;

internal sealed class AddPasskey : IEndpoint
{
    public sealed record AddPasskeyRequest(JsonElement? Credential, string State, string? Name);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("me/passkeys", async (
            AddPasskeyRequest request,
            ICommandHandler<AddPasskeyCommand, PasskeyDetails> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new AddPasskeyCommand(
                PasskeyCredentialJson.From(request.Credential) ?? string.Empty,
                request.State,
                request.Name);

            var result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, Results.Problem);
        })
        .RequireAuthorization()
        .RequirePasswordAuthentication()
        .WithTags(EndpointTags.Users)
        .Produces<PasskeyDetails>()
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
