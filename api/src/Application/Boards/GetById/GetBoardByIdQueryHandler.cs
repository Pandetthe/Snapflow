using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Boards.GetById;

internal sealed class GetBoardByIdQueryHandler(IAppDbContext context) : IQueryHandler<GetBoardByIdQuery, BoardResponse>
{
    public async Task<Result<BoardResponse>> Handle(GetBoardByIdQuery query, CancellationToken cancellationToken = default)
    {
        BoardResponse? board = await context.Boards
            .Where(b => b.Id == query.BoardId)
            .Select(u => new BoardResponse
            {
                Id = u.Id,
                Title = u.Title,
                Description = u.Description,
                CreatedAt = u.CreatedAt,
                CreatedBy = u.CreatedById,
                UpdatedAt = u.UpdatedAt,
                UpdatedBy = u.UpdatedById,
                IsDeleted = u.IsDeleted,
                DeletedAt = u.DeletedAt,
                DeletedBy = u.DeletedById,
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (board is null)
            return Result.Failure<BoardResponse>(UserErrors.NotFound(query.BoardId));

        return board;
    }
}
