using Microsoft.AspNetCore.Mvc;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Users.Avatar;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Users;

internal sealed class Avatar : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/{userId:int}/avatar", async (
                [FromRoute] int userId,
                [FromServices] IQueryHandler<AvatarQuery, AvatarResponse> handler,
                CancellationToken cancellationToken) =>
            {
                var query = new AvatarQuery(userId);

                var result = await handler.Handle(query, cancellationToken);

                return result.Match(x => Results.File(x.Data, x.ContentType), Results.Problem);
            })
            .WithTags(EndpointTags.Users);
    }
}