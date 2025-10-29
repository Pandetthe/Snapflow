﻿using Microsoft.AspNetCore.Mvc;
using Snapflow.Api.Extensions;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Auth.ConfirmEmail;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Auth;

internal sealed class ConfirmEmail : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        string endpointName = string.Empty;
        app.MapGet("auth/confirm-email", async (
            [FromQuery] int userId, [FromQuery] string code, [FromQuery] string? changedEmail,
            [FromQuery] string? redirectUrl,
            ICommandHandler<ConfirmEmailCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ConfirmEmailCommand(
                userId,
                code,
                changedEmail);

            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(
                () =>
                {
                    if (string.IsNullOrWhiteSpace(redirectUrl))
                        return Results.NoContent();
                    return Results.Redirect(redirectUrl);
                },
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