using Snapflow.Api.Extensions;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Auth.SignUp;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Auth;

internal sealed class SignUp : IEndpoint
{
    public sealed record SignUpRequest(string UserName, string Email, string Password);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/sign-up", async (
            SignUpRequest request,
            ICommandHandler<SignUpCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new SignUpCommand(request.UserName, request.Email, request.Password);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Auth);
    }
}