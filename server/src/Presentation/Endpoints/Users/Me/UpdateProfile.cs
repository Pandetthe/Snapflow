using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Users.Me.UpdateProfile;
using Snapflow.Common;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Users.Me;

internal sealed class UpdateProfile : IEndpoint
{
    public sealed record UpdateProfileRequest(string UserName);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("me", async (
            UpdateProfileRequest request,
            ICommandHandler<UpdateProfileCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateProfileCommand(request.UserName);
            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, Results.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Users)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesValidationProblem();
    }
}
