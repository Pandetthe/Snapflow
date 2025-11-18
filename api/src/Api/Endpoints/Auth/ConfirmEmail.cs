using Microsoft.AspNetCore.Mvc;
using Snapflow.Api.Extensions;
using Snapflow.Api.Infrastructure;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Auth.ConfirmEmail;
using Snapflow.Common;
using Snapflow.Infrastructure.Services;

namespace Snapflow.Api.Endpoints.Auth;

internal sealed class ConfirmEmail : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        string endpointName = string.Empty;
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
                CustomResults.Problem
            );
        })
        .WithTags(EndpointTags.Auth)
        .Add(endpointBuilder =>
         {
             var routePattern = ((RouteEndpointBuilder)endpointBuilder).RoutePattern.RawText;
             endpointName = $"{nameof(ConfirmEmail)}-{routePattern}";
             endpointBuilder.Metadata.Add(new EndpointNameMetadata(endpointName));
         });
    }
}