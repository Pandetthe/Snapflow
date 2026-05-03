using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Users.Me.DeleteAccount;
using Snapflow.Common;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Users.Me;

internal sealed class DeleteAccount : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("me", async (
            ICommandHandler<DeleteAccountCommand> handler,
            CancellationToken cancellationToken) =>
        {
            Result result = await handler.Handle(new DeleteAccountCommand(), cancellationToken);
            return result.Match(Results.NoContent, Results.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Users)
        .Produces(StatusCodes.Status204NoContent);
    }
}
