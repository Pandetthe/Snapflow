using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Users.Me.Passkeys.Remove;
using Snapflow.Common;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Users.Me;

internal sealed class RemovePasskey : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("me/passkeys/{passkeyId}", async (
            string passkeyId,
            ICommandHandler<RemovePasskeyCommand> handler,
            CancellationToken cancellationToken) =>
        {
            Result result = await handler.Handle(new RemovePasskeyCommand(passkeyId), cancellationToken);

            return result.Match(Results.NoContent, Results.Problem);
        })
        .RequireAuthorization()
        .RequirePasswordAuthentication()
        .WithTags(EndpointTags.Users)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
