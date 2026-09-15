using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Users.Me.TwoFactor.GetStatus;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Users.Me;

internal sealed class GetTwoFactorStatus : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("me/two-factor", async (
            IQueryHandler<GetTwoFactorStatusQuery, GetTwoFactorStatusResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new GetTwoFactorStatusQuery(), cancellationToken);

            return result.Match(Results.Ok, Results.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Users)
        .Produces<GetTwoFactorStatusResponse>();
    }
}
