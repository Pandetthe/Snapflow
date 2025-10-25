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
            .Include(b => b.CreatedBy)
            .Include(b => b.UpdatedBy)
            .Where(b => !b.IsDeleted && 
                b.Members!.Any(x => x.UserId == userContext.UserId) && (query.Title == null || 
                EF.Functions.ToTsVector("english", b.Title).Matches(EF.Functions.PhraseToTsQuery("english", query.Title))))
            .Select(b => new BoardResponse(
                b.Id, b.Title, b.Description, 
                b.CreatedAt, new User(b.CreatedBy.Id, b.CreatedBy.UserName), 
                b.UpdatedAt, b.UpdatedBy == null ? null : new User(b.UpdatedBy.Id, b.UpdatedBy.UserName)))
            .ToListAsync(cancellationToken);
        return Result.Success(boards);
    }
}
