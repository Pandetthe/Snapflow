using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Lists;

namespace Snapflow.Application.Cards.GetByListId;

internal sealed class GetCardsByListIdQueryHandler(
    IAppDbContext dbContext) : IQueryHandler<GetCardsByListIdQuery, CardsResponse>
{
    public async Task<Result<CardsResponse>> Handle(GetCardsByListIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var list = await dbContext.Lists
            .AsNoTracking()
            .Where(b => b.Id == query.Id && !b.IsDeleted)
            .Select(b => new
            {
                b.Id,
                Cards = new CardsResponse(
                    b.Cards
                    .Where(c => !c.IsDeleted)
                    .Select(c => new CardResponse(
                        c.Id,
                        c.BoardId,
                        c.SwimlaneId,
                        c.ListId,
                        c.Title,
                        c.Description)))
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (list == null)
            return Result.Failure<CardsResponse>(ListErrors.NotFound(query.Id));

        return list.Cards;
    }
}