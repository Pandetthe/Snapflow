using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Cards;
using static Snapflow.Application.Cards.GetById.GetCardByIdResponse;

namespace Snapflow.Application.Cards.GetById;

internal sealed class GetCardByIdHandler(
    IAppDbContext dbContext,
    IBoardMembershipService membershipService) : IQueryHandler<GetCardByIdQuery, GetCardByIdResponse>
{
    public async Task<Result<GetCardByIdResponse>> Handle(GetCardByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        bool isMember = await membershipService.IsMemberAsync(query.BoardId, cancellationToken);

        GetCardByIdResponse? card = await dbContext.Cards
            .AsNoTracking()
            .Where(c => c.Id == query.Id && c.BoardId == query.BoardId && !c.IsDeleted)
            .Select(c => new GetCardByIdResponse(
                c.Id,
                c.ListId,
                c.SwimlaneId,
                c.BoardId,
                c.Title,
                c.Description,
                c.Rank,
                c.CreatedAt,
                isMember ? UserDto.From(c.CreatedBy) : null,
                c.UpdatedAt,
                isMember ? UserDto.From(c.UpdatedBy) : null))
            .SingleOrDefaultAsync(cancellationToken);

        if (card == null)
            return Result.Failure<GetCardByIdResponse>(CardErrors.NotFound(query.Id));

        return card;
    }
}