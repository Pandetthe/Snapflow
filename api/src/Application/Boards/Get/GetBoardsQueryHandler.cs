using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;

namespace Snapflow.Application.Boards.Get;

internal sealed class GetBoardsQueryHandler(
    IAppDbContext dbContext,
    IUserContext userContext) : IQueryHandler<GetBoardsQuery, BoardsResponse>
{
    public async Task<Result<BoardsResponse>> Handle(GetBoardsQuery query, CancellationToken cancellationToken = default)
    {
        var boards = await dbContext.Boards
            .Where(b => !b.IsDeleted && b.Members!.Any(x => x.UserId == userContext.UserId))
            .Where(b => query.Title == null || 
                EF.Functions.ToTsVector("english", b.Title).Matches(EF.Functions.PhraseToTsQuery("english", query.Title)))
            .Select(b => new BoardResponse(
                b.Id, 
                b.Title, 
                b.Description, 
                b.CreatedAt, 
                new UserResponse(b.CreatedBy.Id, b.CreatedBy.UserName), 
                b.UpdatedAt, 
                b.UpdatedBy == null ? null : new UserResponse(b.UpdatedBy.Id, b.UpdatedBy.UserName)))
            .ToListAsync(cancellationToken);
        return new BoardsResponse(boards);
    }
}
