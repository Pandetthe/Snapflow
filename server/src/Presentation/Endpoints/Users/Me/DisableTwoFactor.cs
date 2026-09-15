using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Users.Me.TwoFactor.Disable;
using Snapflow.Common;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Users.Me;

internal sealed class DisableTwoFactor : IEndpoint
{
    public sealed record DisableTwoFactorRequest(string? Code, string? RecoveryCode);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("me/two-factor/disable", async (
            DisableTwoFactorRequest request,
            ICommandHandler<DisableTwoFactorCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new DisableTwoFactorCommand(request.Code, request.RecoveryCode);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, Results.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Users)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem();
    }
}
