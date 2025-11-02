using Snapflow.Api.Extensions;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Swimlanes.GetById;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Swimlanes;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards/{boardId:int}/swimlanes/{swimlaneId:int}", async (
            int boardId, int swimlaneId,
            IQueryHandler<GetSwimlaneByIdQuery, SwimlaneResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetSwimlaneByIdQuery(swimlaneId);

            Result<SwimlaneResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Swimlanes);
    }
}
