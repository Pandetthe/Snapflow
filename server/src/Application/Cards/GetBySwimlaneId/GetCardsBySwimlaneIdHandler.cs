using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;
using static Snapflow.Application.Cards.GetBySwimlaneId.GetCardsBySwimlaneIdResponse;

namespace Snapflow.Application.Cards.GetBySwimlaneId;

internal sealed class GetCardsBySwimlaneIdQueryHandler(
    IAppDbContext dbContext) : IQueryHandler<GetCardsBySwimlaneIdQuery, IReadOnlyList<CardDto>>
{
    public async Task<Result<IReadOnlyList<CardDto>>> Handle(GetCardsBySwimlaneIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var swimlane = await dbContext.Swimlanes
            .AsNoTracking()
            .Where(b => b.Id == query.Id && !b.IsDeleted)
            .Select(b => new
            {
                b.Id,
                Cards = b.Cards
                    .Where(c => !c.IsDeleted)
                    .OrderBy(c => c.Rank)
                    .Select(c => new CardDto(
                        c.Id,
                        c.BoardId,
                        c.SwimlaneId,
                        c.ListId,
                        c.Title,
                        c.Description,
                        c.Rank))
                    .ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (swimlane == null)
            return Result.Failure<IReadOnlyList<CardDto>>(SwimlaneErrors.NotFound(query.Id));

        return swimlane.Cards;
    }
}