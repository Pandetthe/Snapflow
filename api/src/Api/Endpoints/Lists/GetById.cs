using Snapflow.Api.Extensions;
using Snapflow.Api.Infrastructure;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Lists.GetById;
using Snapflow.Common;
using Snapflow.Domain.Boards;

namespace Snapflow.Api.Endpoints.Lists;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("boards/{boardId:int}/kists/{listId:int}", async (
            int boardId, int listId,
            IQueryHandler<GetListByIdQuery, GetListByIdResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = new GetListByIdQuery(listId);

            Result<GetListByIdResponse> result = await handler.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .RequireAuthorization(BoardPermissions.Boards.View)
        .WithTags(EndpointTags.Lists)
        .Produces<GetListByIdResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
