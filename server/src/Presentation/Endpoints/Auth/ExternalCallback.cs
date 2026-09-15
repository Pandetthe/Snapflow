using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Auth.ExternalSignIn;
using Snapflow.Common;
using Snapflow.Infrastructure.Common;

namespace Snapflow.Presentation.Endpoints.Auth;

internal sealed class ExternalCallback : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("auth/external/callback", async (
            ICommandHandler<ExternalSignInCommand> handler,
            ServiceLinkBuilder serviceLinkBuilder,
            CancellationToken cancellationToken) =>
        {
            Result result = await handler.Handle(new ExternalSignInCommand(), cancellationToken);

            Uri target = result.IsSuccess
                ? serviceLinkBuilder.BuildWebLink("/boards")
                : serviceLinkBuilder.BuildWebLink("/sign-in", $"error={Uri.EscapeDataString(result.Error.Code)}");

            return Results.Redirect(target.ToString());
        })
        .AllowAnonymous()
        .WithTags(EndpointTags.Auth)
        .Produces(StatusCodes.Status302Found);
    }
}
