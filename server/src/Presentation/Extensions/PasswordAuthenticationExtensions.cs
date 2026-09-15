using Snapflow.Application.Abstractions.Identity;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Presentation.Extensions;

public static class PasswordAuthenticationExtensions
{
    public static RouteHandlerBuilder RequirePasswordAuthentication(this RouteHandlerBuilder builder) =>
        builder
            .AddEndpointFilter(async (context, next) =>
            {
                var settings = context.HttpContext.RequestServices.GetRequiredService<IAuthenticationSettings>();
                return settings.PasswordAuthenticationEnabled
                    ? await next(context)
                    : Results.Problem(Result.Failure(AuthenticationErrors.PasswordAuthenticationDisabled));
            })
            .ProducesProblem(StatusCodes.Status404NotFound);
}
