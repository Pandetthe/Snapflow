using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Boards;

namespace Snapflow.Application.Boards.GetPublic;

internal sealed class GetPublicBoardsHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    IBoardVisibilityPolicy visibilityPolicy) : IQueryHandler<GetPublicBoardsQuery, IReadOnlyList<GetPublicBoardsResponse>>
{
    private const int Limit = 60;

    public async Task<Result<IReadOnlyList<GetPublicBoardsResponse>>> Handle(GetPublicBoardsQuery query, CancellationToken cancellationToken = default)
    {
        bool isAuthenticated = userContext.IsAuthenticated;
        List<BoardVisibility> visibilities = Enum.GetValues<BoardVisibility>()
            .Where(visibility => visibilityPolicy.IsListed(visibility)
                && visibilityPolicy.CanNonMemberView(visibility, isAuthenticated))
            .ToList();
        if (visibilities.Count == 0)
            return Result.Success<IReadOnlyList<GetPublicBoardsResponse>>([]);

        IQueryable<Board> boards = dbContext.Boards
            .AsNoTracking()
            .Where(b => !b.IsDeleted && visibilities.Contains(b.Visibility));

        if (isAuthenticated)
        {
            int userId = userContext.UserId;
            boards = boards.Where(b => !b.Members.Any(m => m.UserId == userId));
        }

        return await boards
            .OrderByDescending(b => b.UpdatedAt ?? b.CreatedAt)
            .Take(Limit)
            .Select(b => new GetPublicBoardsResponse(b.Id, b.Title, b.Description))
            .ToListAsync(cancellationToken);
    }
}
