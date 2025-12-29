using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Application.Ranking;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Swimlanes;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Swimlanes.Create;

internal sealed class CreateSwimlaneHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider,
    IEntityRankService<Swimlane> rankService) : ICommandHandler<CreateSwimlaneCommand, int>
{
    public async Task<Result<int>> Handle(CreateSwimlaneCommand command, CancellationToken cancellationToken = default)
    {
        var userExists = await dbContext.Users.AsNoTracking()
            .AnyAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (!userExists)
            return Result.Failure<int>(UserErrors.NotFound(userContext.UserId));

        var boardExists = await dbContext.Boards.AsNoTracking()
            .AnyAsync(b => b.Id == command.BoardId && !b.IsDeleted, cancellationToken);
        if (!boardExists)
            return Result.Failure<int>(BoardErrors.NotFound(command.BoardId));
        Result<string> rankResult = await rankService.GenerateRankAsync(
            command.BoardId, null, command.BeforeId, cancellationToken);
        if (!rankResult.IsSuccess)
            return Result.Failure<int>(rankResult.Error);

        var swimlane = new Swimlane
        {
            BoardId = command.BoardId,
            Title = command.Title,
            Rank = rankResult.Value,
            Height = command.Height,
            CreatedById = userContext.UserId,
            CreatedAt = timeProvider.GetUtcNow(),
        };

        swimlane.Raise(new SwimlaneCreatedDomainEvent(swimlane.BoardId, swimlane.Title));

        await dbContext.Swimlanes.AddAsync(swimlane, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return swimlane.Id;
    }
}