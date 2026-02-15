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
        var boardsQuery = dbContext.Boards
            .AsNoTracking()
            .Where(b => !b.IsDeleted && b.Members.Any(x => x.UserId == userContext.UserId));

        if (!string.IsNullOrWhiteSpace(query.Title))
        {
            boardsQuery = boardsQuery.Where(b =>
                EF.Functions.ToTsVector("english", b.Title)
                .Matches(EF.Functions.PhraseToTsQuery("english", query.Title)));
        }

        return await boardsQuery
            .OrderBy(b => b.Title)
            .Select(b => new BoardDto(
                b.Id,
                b.Title,
                b.Description,
                b.CreatedAt,
                UserDto.From(b.CreatedBy),
                b.UpdatedAt,
                UserDto.From(b.UpdatedBy)))
            .ToListAsync(cancellationToken);
    }
}
