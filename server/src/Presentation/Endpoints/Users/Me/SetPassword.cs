using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Users.Me.SetPassword;
using Snapflow.Common;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Users.Me;

internal sealed class SetPassword : IEndpoint
{
    public sealed record SetPasswordRequest(string NewPassword);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("me/password", async (
            SetPasswordRequest request,
            ICommandHandler<SetPasswordCommand> handler,
            CancellationToken cancellationToken) =>
        {
            Result result = await handler.Handle(new SetPasswordCommand(request.NewPassword), cancellationToken);
            return result.Match(Results.NoContent, Results.Problem);
        })
        .RequireAuthorization()
        .RequirePasswordAuthentication()
        .WithTags(EndpointTags.Users)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesValidationProblem();
    }
}
