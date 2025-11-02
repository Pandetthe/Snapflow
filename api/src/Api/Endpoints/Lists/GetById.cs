using Snapflow.Api.Extensions;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Lists.GetById;
using Snapflow.Common;

namespace Snapflow.Api.Endpoints.Lists;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards/{boardId:int}/Lists/{listId:int}", async (
            int boardId, int listId,
            IQueryHandler<GetListByIdQuery, ListResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetListByIdQuery(listId);

            Result<ListResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization()
        .WithTags(EndpointTags.Lists);
    }
}
