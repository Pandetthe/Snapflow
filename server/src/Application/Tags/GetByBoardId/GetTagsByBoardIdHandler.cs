using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using static Snapflow.Application.Tags.GetByBoardId.GetTagsByBoardIdResponse;

namespace Snapflow.Application.Tags.GetByBoardId;

internal sealed class GetTagsByBoardIdHandler(
    IAppDbContext dbContext) : IQueryHandler<GetTagsByBoardIdQuery, IReadOnlyList<TagDto>>
{
    public async Task<Result<IReadOnlyList<TagDto>>> Handle(GetTagsByBoardIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var board = await dbContext.Boards
            .AsNoTracking()
            .Where(b => b.Id == query.Id && !b.IsDeleted)
            .Select(b => new
            {
                b.Id,
                Tags = b.Tags
                    .Where(t => !t.IsDeleted)
                    .OrderBy(t => t.Title)
                    .Select(t => new TagDto(t.Id, t.Title, t.Color))
                    .ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (board == null)
            return Result.Failure<IReadOnlyList<TagDto>>(BoardErrors.NotFound(query.Id));
        return board.Tags;
    }
}
