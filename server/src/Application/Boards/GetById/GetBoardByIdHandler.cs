using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using static Snapflow.Application.Boards.GetById.GetBoardByIdResponse;

namespace Snapflow.Application.Boards.GetById;

internal sealed class GetBoardByIdHandler(
    IAppDbContext context) : IQueryHandler<GetBoardByIdQuery, GetBoardByIdResponse>
{
    public async Task<Result<GetBoardByIdResponse>> Handle(GetBoardByIdQuery query, CancellationToken cancellationToken = default)
    {
        var board = await context.Boards
            .AsNoTracking()
            .Where(b => b.Id == query.Id && !b.IsDeleted)
            .Select(b => new GetBoardByIdResponse(
                b.Id,
                b.Title,
                b.Description,
                b.Swimlanes
                    .Where(s => !s.IsDeleted)
                    .OrderBy(s => s.Rank)
                    .Select(s => new SwimlaneDto(
                        s.Id,
                        s.Title,
                        s.Rank,
                        s.Height,
                        s.Lists
                            .Where(l => !l.IsDeleted)
                            .OrderBy(l => l.Rank)
                            .Select(l => new ListDto(
                                l.Id,
                                l.Title,
                                l.Rank,
                                l.Width,
                                l.Cards
                                    .Where(c => !c.IsDeleted)
                                    .OrderBy(c => c.Rank)
                                    .Select(c => new CardDto(
                                        c.Id,
                                        c.Title,
                                        c.Description,
                                        c.Rank,
                                        c.CreatedAt,
                                        UserDto.From(c.CreatedBy),
                                        c.UpdatedAt,
                                        UserDto.From(c.UpdatedBy)))
                                    .ToList()))
                            .ToList()))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);

        if (board == null)
            return Result.Failure<GetBoardByIdResponse>(BoardErrors.NotFound(query.Id));

        return board;
    }
}
