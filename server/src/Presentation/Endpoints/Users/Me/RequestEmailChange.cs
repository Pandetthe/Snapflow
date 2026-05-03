using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Users.Me.RequestEmailChange;
using Snapflow.Common;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Users.Me;

internal sealed class RequestEmailChange : IEndpoint
{
    public sealed record RequestEmailChangeRequest(string NewEmail);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("me/email", async (
            RequestEmailChangeRequest request,
            ICommandHandler<RequestEmailChangeCommand> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RequestEmailChangeCommand(request.NewEmail);
            Result result = await handler.Handle(command, cancellationToken);
            return result.Match(Results.NoContent, Results.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Users)
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesValidationProblem();
    }
}
