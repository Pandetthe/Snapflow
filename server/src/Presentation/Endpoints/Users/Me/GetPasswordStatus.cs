using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Users.Me.GetPasswordStatus;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Users.Me;

internal sealed class GetPasswordStatus : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("me/password", async (
            IQueryHandler<GetPasswordStatusQuery, GetPasswordStatusResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new GetPasswordStatusQuery(), cancellationToken);

            return result.Match(Results.Ok, Results.Problem);
        })
        .RequireAuthorization()
        .RequirePasswordAuthentication()
        .WithTags(EndpointTags.Users)
        .Produces<GetPasswordStatusResponse>();
    }
}
