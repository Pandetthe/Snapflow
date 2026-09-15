using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Users.Me.TwoFactor.Enable;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Users.Me;

internal sealed class EnableTwoFactor : IEndpoint
{
    public sealed record EnableTwoFactorRequest(string Code);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("me/two-factor/enable", async (
            EnableTwoFactorRequest request,
            ICommandHandler<EnableTwoFactorCommand, EnableTwoFactorResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new EnableTwoFactorCommand(request.Code), cancellationToken);

            return result.Match(Results.Ok, Results.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Users)
        .Produces<EnableTwoFactorResponse>()
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
