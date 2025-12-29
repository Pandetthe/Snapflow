using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Lists;
using System.Collections.Generic;
using static Snapflow.Application.Cards.GetByListId.GetCardsByListIdResponse;

namespace Snapflow.Application.Cards.GetByListId;

internal sealed class GetCardsByListIdHandler(
    IAppDbContext dbContext) : IQueryHandler<GetCardsByListIdQuery, IReadOnlyList<CardDto>>
{
    public async Task<Result<IReadOnlyList<CardDto>>> Handle(GetCardsByListIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var list = await dbContext.Lists
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

        if (list == null)
            return Result.Failure<IReadOnlyList<CardDto>> (ListErrors.NotFound(query.Id));

        return list.Cards;
    }
}