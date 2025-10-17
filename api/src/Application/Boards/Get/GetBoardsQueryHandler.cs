using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;

namespace Snapflow.Application.Boards.Get;

internal sealed class GetBoardsQueryHandler(
    IAppDbContext dbContext)
    : IQueryHandler<GetBoardsQuery, List<BoardResponse>>
{
    public async Task<Result<List<BoardResponse>>> Handle(GetBoardsQuery query, CancellationToken cancellationToken = default)
    {
        var boards = await dbContext.Boards
            .Where(b => !b.IsDeleted && (query.Title == null || 
                EF.Functions.ToTsVector("english", b.Title)
                .Matches(EF.Functions.PhraseToTsQuery("english", query.Title))))
            .Select(x => new BoardResponse
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description
            })
            .ToListAsync(cancellationToken);
        return Result.Success(boards);
    }
}
