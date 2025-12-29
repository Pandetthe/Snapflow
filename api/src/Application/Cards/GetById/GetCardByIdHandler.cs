using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Cards;
using static Snapflow.Application.Cards.GetById.GetCardByIdResponse;

namespace Snapflow.Application.Cards.GetById;

internal sealed class GetCardByIdHandler(
    IAppDbContext dbContext) : IQueryHandler<GetCardByIdQuery, GetCardByIdResponse>
{
    public async Task<Result<GetCardByIdResponse>> Handle(GetCardByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var card = await dbContext.Cards
            .AsNoTracking()
            .Where(c => c.Id == query.Id && !c.IsDeleted)
            .Select(c => new GetCardByIdResponse(
                c.Id,
                c.BoardId,
                c.SwimlaneId,
                c.ListId,
                c.Title,
                c.Description,
                c.Rank,
                c.CreatedAt,
                UserDto.From(c.CreatedBy),
                c.UpdatedAt,
                UserDto.From(c.UpdatedBy)))
            .SingleOrDefaultAsync(cancellationToken);

        if (card == null)
            return Result.Failure<GetCardByIdResponse>(CardErrors.NotFound(query.Id));

        return card;
    }
}