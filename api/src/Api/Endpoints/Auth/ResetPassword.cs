using Snapflow.Api.Extensions;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Auth.ResetPassword;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Auth;

internal sealed class ResetPassword : IEndpoint
{
    public sealed record ResetPasswordRequest(string Email, string ResetCode, string NewPassword);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/reset-password", async (
            ResetPasswordRequest request,
            ICommandHandler<ResetPasswordCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ResetPasswordCommand(
                request.Email, 
                request.ResetCode,
                request.NewPassword);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Auth);
    }
}
