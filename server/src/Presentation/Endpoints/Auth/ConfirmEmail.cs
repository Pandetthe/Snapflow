using Microsoft.AspNetCore.Mvc;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Auth.ConfirmEmail;
using Snapflow.Common;
using Snapflow.Infrastructure.Common;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Auth;

internal sealed class ConfirmEmail : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("auth/confirm-email", async (
            [FromQuery] string email, [FromQuery] string code, [FromQuery] string? changedEmail,
            ServiceLinkBuilder serviceLinkBuilder,
            ICommandHandler<ConfirmEmailCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ConfirmEmailCommand(
                email,
                code,
                changedEmail);

            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(
                () => Results.Redirect(serviceLinkBuilder.BuildEmailConfirmationRedirect().ToString()),
                Results.Problem
            );
        })
        .AllowAnonymous()
        .WithTags(EndpointTags.Auth)
        .Produces(StatusCodes.Status302Found)
        .ProducesCustomValidationProblem();
    }
}