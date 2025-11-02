using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Cards;

namespace Snapflow.Application.Cards.GetById;

internal sealed class GetCardsByIdQueryHandler(
    IAppDbContext dbContext) : IQueryHandler<GetCardsByIdQuery, CardResponse>
{
    public async Task<Result<CardResponse>> Handle(GetCardsByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var card = await dbContext.Cards
            .AsNoTracking()
            .Where(c => c.Id == query.Id && !c.IsDeleted)
            .Select(c => new CardResponse(
                c.Id,
                c.BoardId,
                c.SwimlaneId,
                c.ListId,
                c.Title,
                c.Description,
                c.CreatedAt,
                new UserResponse(c.CreatedBy.Id, c.CreatedBy.UserName),
                c.UpdatedAt,
                c.UpdatedBy == null ? null : new UserResponse(c.UpdatedBy.Id, c.UpdatedBy.UserName)))
            .SingleOrDefaultAsync(cancellationToken);

        if (card == null)
            return Result.Failure<CardResponse>(CardErrors.NotFound(query.Id));

        return card;
    }
}