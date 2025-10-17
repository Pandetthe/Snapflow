using Snapflow.Domain.Users;

namespace Snapflow.Api.Endpoints.Auth;

internal sealed class IdentityApiGroup : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGroup("auth").WithTags(Tags.Auth).MapIdentityApi<User>();
    }
}
