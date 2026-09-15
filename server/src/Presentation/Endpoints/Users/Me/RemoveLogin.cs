using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Users.Me.Logins.Remove;
using Snapflow.Common;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Users.Me;

internal sealed class RemoveLogin : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("me/logins/{provider}", async (
            string provider,
            ICommandHandler<RemoveLoginCommand> handler,
            CancellationToken cancellationToken) =>
        {
            Result result = await handler.Handle(new RemoveLoginCommand(provider), cancellationToken);

            return result.Match(Results.NoContent, Results.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Users)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
