using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;

namespace Snapflow.Application.Boards.GetById;

internal sealed class GetBoardByIdQueryHandler(
    IAppDbContext context) : IQueryHandler<GetBoardByIdQuery, BoardResponse>
{
    public async Task<Result<BoardResponse>> Handle(GetBoardByIdQuery query, CancellationToken cancellationToken = default)
    {
        BoardResponse? board = await context.Boards
            .AsNoTracking()
            .Where(b => b.Id == query.Id && !b.IsDeleted)
            .Select(b => new BoardResponse(
                b.Id,
                b.Title,
                b.Description,
                new SwimlanesResponse(b.Swimlanes
                    .Where(s => !s.IsDeleted)
                    .OrderBy(s => s.Rank)
                    .Select(s => new SwimlaneResponse(
                        s.Id,
                        s.Title,
                        s.Rank,
                        new ListsResponse(s.Lists
                            .Where(l => !l.IsDeleted)
                            .Select(l => new ListResponse(
                                l.Id,
                                l.Title,
                                l.CreatedAt,
                                new UserResponse(l.CreatedBy.Id, l.CreatedBy.UserName),
                                l.UpdatedAt,
                                l.UpdatedBy == null ? null : new UserResponse(l.UpdatedBy.Id, l.UpdatedBy.UserName)))),
                        s.CreatedAt,
                        new UserResponse(s.CreatedBy.Id, s.CreatedBy.UserName),
                        s.UpdatedAt,
                        s.UpdatedBy == null ? null : new UserResponse(s.UpdatedBy.Id, s.UpdatedBy.UserName)))),
                b.CreatedAt,
                new UserResponse(b.CreatedBy.Id, b.CreatedBy.UserName),
                b.UpdatedAt,
                b.UpdatedBy == null ? null : new UserResponse(b.UpdatedBy.Id, b.UpdatedBy.UserName)))
            .SingleOrDefaultAsync(cancellationToken);

        if (board == null)
            return Result.Failure<BoardResponse>(BoardErrors.NotFound(query.Id));

        return board;
    }
}
