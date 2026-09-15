using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Users.Me.Passkeys.Rename;
using Snapflow.Common;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Users.Me;

internal sealed class RenamePasskey : IEndpoint
{
    public sealed record RenamePasskeyRequest(string Name);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("me/passkeys/{passkeyId}", async (
            string passkeyId,
            RenamePasskeyRequest request,
            ICommandHandler<RenamePasskeyCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RenamePasskeyCommand(passkeyId, request.Name);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, Results.Problem);
        })
        .RequireAuthorization()
        .RequirePasswordAuthentication()
        .WithTags(EndpointTags.Users)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
