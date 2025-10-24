using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;

namespace Snapflow.Application.Boards.Get;

internal sealed class GetBoardsQueryHandler(
    IAppDbContext dbContext,
    IUserContext userContext)
    : IQueryHandler<GetBoardsQuery, List<BoardResponse>>
{
    public async Task<Result<List<BoardResponse>>> Handle(GetBoardsQuery query, CancellationToken cancellationToken = default)
    {
        var boards = await dbContext.Boards
            .Include(b => b.Members)
            .Where(b => !b.IsDeleted && 
                b.Members!.Any(x => x.UserId == userContext.UserId) && (query.Title == null || 
                EF.Functions.ToTsVector("english", b.Title).Matches(EF.Functions.PhraseToTsQuery("english", query.Title))))
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
