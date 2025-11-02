using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;

namespace Snapflow.Application.Cards.GetBySwimlaneId;

internal sealed class GetCardsBySwimlaneIdQueryHandler(
    IAppDbContext dbContext) : IQueryHandler<GetCardsBySwimlaneIdQuery, CardsResponse>
{
    public async Task<Result<CardsResponse>> Handle(GetCardsBySwimlaneIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var swimlane = await dbContext.Swimlanes
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

        if (swimlane == null)
            return Result.Failure<CardsResponse>(SwimlaneErrors.NotFound(query.Id));

        return swimlane.Cards;
    }
}