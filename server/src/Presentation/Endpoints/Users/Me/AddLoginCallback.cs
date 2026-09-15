using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Users.Me.Logins.Add;
using Snapflow.Common;
using Snapflow.Infrastructure.Common;

namespace Snapflow.Presentation.Endpoints.Users.Me;

internal sealed class AddLoginCallback : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("me/logins/callback", async (
            ICommandHandler<AddLoginCommand, string> handler,
            ServiceLinkBuilder serviceLinkBuilder,
            CancellationToken cancellationToken) =>
        {
            Result<string> result = await handler.Handle(new AddLoginCommand(), cancellationToken);

            Uri target = result.IsSuccess
                ? serviceLinkBuilder.BuildWebLink("/profile", $"linked={Uri.EscapeDataString(result.Value)}")
                : serviceLinkBuilder.BuildWebLink("/profile", $"error={Uri.EscapeDataString(result.Error.Code)}");

            return Results.Redirect(target.ToString());
        })
        .AllowAnonymous()
        .WithTags(EndpointTags.Users)
        .Produces(StatusCodes.Status302Found);
    }
}
