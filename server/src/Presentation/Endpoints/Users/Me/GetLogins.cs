using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Users.Me.Logins.GetAll;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Users.Me;

internal sealed class GetLogins : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("me/logins", async (
            IQueryHandler<GetLoginsQuery, IReadOnlyList<ExternalLoginDetails>> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new GetLoginsQuery(), cancellationToken);

            return result.Match(Results.Ok, Results.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Users)
        .Produces<IReadOnlyList<ExternalLoginDetails>>();
    }
}
