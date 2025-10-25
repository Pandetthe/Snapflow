using Snapflow.Api.Extensions;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Auth.ForgotPassword;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Auth;

internal sealed class ForgotPassword : IEndpoint
{
    public sealed record ForgotPasswordRequest(string Email);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/forgot-password", async (
            ForgotPasswordRequest request,
            ICommandHandler<ForgotPasswordCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ForgotPasswordCommand(request.Email);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Auth);
    }
}
