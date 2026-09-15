using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Snapflow.Common;
using Snapflow.Domain.Users;
using Snapflow.Infrastructure.Auth.Entities;
using Snapflow.Infrastructure.Auth.External;
using Snapflow.Infrastructure.Common;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Users.Me;

internal sealed class AddLogin : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("me/logins/{scheme}", (
            string scheme,
            HttpContext context,
            ExternalProviderRegistry registry,
            SignInManager<AppUser> signInManager,
            ServiceLinkBuilder serviceLinkBuilder) =>
        {
            if (registry.FindRedirectProvider(scheme) is null)
                return Results.Problem(Result.Failure(AuthenticationErrors.ProviderNotAvailable));

            AuthenticationProperties properties = signInManager.ConfigureExternalAuthenticationProperties(
                scheme,
                serviceLinkBuilder.BuildExternalLinkCallbackLink().ToString(),
                signInManager.UserManager.GetUserId(context.User));

            return Results.Challenge(properties, [scheme]);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Users)
        .Produces(StatusCodes.Status302Found)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
