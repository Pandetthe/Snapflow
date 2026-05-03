using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using static Snapflow.Application.Boards.Get.GetBoardsResponse;

namespace Snapflow.Application.Boards.Get;

internal sealed class GetBoardsHandler(
    IAppDbContext dbContext,
    IUserContext userContext) : IQueryHandler<GetBoardsQuery, IReadOnlyList<BoardDto>>
{
    public async Task<Result<IReadOnlyList<BoardDto>>> Handle(GetBoardsQuery query, CancellationToken cancellationToken = default)
    {
        var memberships = dbContext.Members
            .AsNoTracking()
            .Where(m => m.UserId == userContext.UserId);

        var boardsQuery = memberships
            .Join(
                dbContext.Boards.AsNoTracking().Where(b => !b.IsDeleted),
                m => m.BoardId,
                b => b.Id,
                (m, b) => new { m.Role, Board = b });

        if (!string.IsNullOrWhiteSpace(query.Title))
        {
            boardsQuery = boardsQuery.Where(x =>
                EF.Functions.ToTsVector("english", x.Board.Title)
                .Matches(EF.Functions.PhraseToTsQuery("english", query.Title)));
        }

        return await boardsQuery
            .OrderBy(x => x.Board.Title)
            .Select(x => new BoardDto(
                x.Board.Id,
                x.Board.Title,
                x.Board.Description,
                x.Role,
                x.Board.CreatedAt,
                UserDto.From(x.Board.CreatedBy),
                x.Board.UpdatedAt,
                UserDto.From(x.Board.UpdatedBy)))
            .ToListAsync(cancellationToken);
    }
}
