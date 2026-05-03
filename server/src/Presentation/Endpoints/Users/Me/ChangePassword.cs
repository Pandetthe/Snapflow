using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Users.Me.ChangePassword;
using Snapflow.Common;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Users.Me;

internal sealed class ChangePassword : IEndpoint
{
    public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("me/password", async (
            ChangePasswordRequest request,
            ICommandHandler<ChangePasswordCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ChangePasswordCommand(request.CurrentPassword, request.NewPassword);
            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, Results.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Users)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem();
    }
}
