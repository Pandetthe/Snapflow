using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Users.Me.TwoFactor.RegenerateRecoveryCodes;
using Snapflow.Presentation.Extensions;

namespace Snapflow.Presentation.Endpoints.Users.Me;

internal sealed class RegenerateRecoveryCodes : IEndpoint
{
    public sealed record RegenerateRecoveryCodesRequest(string Code);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("me/two-factor/recovery-codes", async (
            RegenerateRecoveryCodesRequest request,
            ICommandHandler<RegenerateRecoveryCodesCommand, RegenerateRecoveryCodesResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new RegenerateRecoveryCodesCommand(request.Code), cancellationToken);

            return result.Match(Results.Ok, Results.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Users)
        .Produces<RegenerateRecoveryCodesResponse>()
        .ProducesValidationProblem();
    }
}
