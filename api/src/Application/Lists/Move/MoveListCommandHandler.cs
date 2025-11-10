using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Application.Ranking;
using Snapflow.Common;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Lists.Move;

internal sealed class MoveListCommandHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider,
    IEntityRankService<List> rankService) : ICommandHandler<MoveListCommand>
{
    public async Task<Result> Handle(MoveListCommand command, CancellationToken cancellationToken = default)
    {
        var userExists = await dbContext.Users.AsNoTracking()
            .AnyAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (!userExists)
            return Result.Failure<int>(UserErrors.NotFound(userContext.UserId));

        var list = await dbContext.Lists
            .SingleOrDefaultAsync(s => s.Id == command.Id && !s.IsDeleted, cancellationToken);
        if (list == null)
            return Result.Failure(ListErrors.NotFound(command.Id));
        Result<string> rankResult = await rankService.GenerateRankAsync(
            list.SwimlaneId, command.Id, command.BeforeId, cancellationToken);
        if (!rankResult.IsSuccess)
            return Result.Failure(rankResult.Error);
        list.Rank = rankResult.Value;
        list.UpdatedById = userContext.UserId;
        list.UpdatedAt = timeProvider.GetUtcNow();
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}