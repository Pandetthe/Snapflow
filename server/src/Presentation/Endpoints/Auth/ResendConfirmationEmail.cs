using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Auth.ResendConfirmationEmail;
using Snapflow.Common;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Auth;

internal sealed class ResendConfirmationEmail : IEndpoint
{
    public sealed class ResendConfirmationEmailRequest
    {
        public required string Email { get; init; }
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        string endpointName = string.Empty;
        app.MapPost("auth/resend-confirmation-email", async (
            ResendConfirmationEmailRequest request,
            ICommandHandler<ResendConfirmationEmailCommand> handler,
            HttpContext context,
            CancellationToken cancellationToken) =>
        {
            var command = new ResendConfirmationEmailCommand(request.Email);

            Result result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.NoContent, Results.Problem
            );
        })
        .WithTags(EndpointTags.Auth);
    }
}
