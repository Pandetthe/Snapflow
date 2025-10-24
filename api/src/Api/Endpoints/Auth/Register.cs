using Snapflow.Api.Extensions;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Auth.Register;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Auth;

internal sealed class Register : IEndpoint
{
    public sealed class RegisterRequest
    {
        public required string UserName { get; init; }

        public required string Email { get; init; }

        public required string Password { get; init; }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/register", async (
            RegisterRequest request,
            ICommandHandler<RegisterCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RegisterCommand(
                request.UserName,
                request.Email,
                request.Password);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Auth);
    }
}