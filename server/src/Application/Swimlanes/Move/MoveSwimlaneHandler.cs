using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Application.Ranking;
using Snapflow.Common;
using Snapflow.Domain.Swimlanes;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Swimlanes.Move;

internal sealed class MoveSwimlaneHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider,
    IEntityRankService<Swimlane> rankService) : ICommandHandler<MoveSwimlaneCommand, string>
{
    public async Task<Result<string>> Handle(MoveSwimlaneCommand command, CancellationToken cancellationToken = default)
    {
        IUser? user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (user == null)
            return Result.Failure<string>(UserErrors.NotFound(userContext.UserId));

        Swimlane? swimlane = await dbContext.Swimlanes
            .SingleOrDefaultAsync(s => s.Id == command.Id && s.BoardId == command.BoardId && !s.IsDeleted, cancellationToken);
        if (swimlane == null)
            return Result.Failure<string>(SwimlaneErrors.NotFound(command.Id));
        var rankResult = await rankService.GenerateRankAsync(
            swimlane.BoardId, command.Id, command.BeforeId, cancellationToken);
        if (!rankResult.IsSuccess)
            return Result.Failure<string>(rankResult.Error);
        
        swimlane.Move(
            rankResult.Value,
            user,
            timeProvider.GetUtcNow(),
            userContext.ConnectionId);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success(rankResult.Value);
    }
}