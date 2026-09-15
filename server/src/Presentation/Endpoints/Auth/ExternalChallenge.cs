using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Snapflow.Common;
using Snapflow.Domain.Users;
using Snapflow.Infrastructure.Auth.Entities;
using Snapflow.Infrastructure.Auth.External;
using Snapflow.Infrastructure.Common;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Auth;

internal sealed class ExternalChallenge : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("auth/external/{scheme}", (
            string scheme,
            bool? rememberMe,
            ExternalProviderRegistry registry,
            SignInManager<AppUser> signInManager,
            ServiceLinkBuilder serviceLinkBuilder) =>
        {
            if (registry.FindRedirectProvider(scheme) is null)
                return Results.Problem(Result.Failure(AuthenticationErrors.ProviderNotAvailable));

            AuthenticationProperties properties = signInManager.ConfigureExternalAuthenticationProperties(
                scheme,
                serviceLinkBuilder.BuildExternalSignInCallbackLink().ToString());
            properties.Items[ExternalProviderRegistry.PersistentItemKey] = rememberMe == true ? "true" : "false";

            return Results.Challenge(properties, [scheme]);
        })
        .AllowAnonymous()
        .WithTags(EndpointTags.Auth)
        .Produces(StatusCodes.Status302Found)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
