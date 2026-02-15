using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Application.Ranking;
using Snapflow.Common;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Swimlanes;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Lists.Create;

internal sealed class CreateListHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider,
    IEntityRankService<List> rankService) : ICommandHandler<CreateListCommand, int>
{
    public async Task<Result<int>> Handle(CreateListCommand command, CancellationToken cancellationToken = default)
    {
        var userExists = await dbContext.Users.AsNoTracking()
            .AnyAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (!userExists)
            return Result.Failure<int>(UserErrors.NotFound(userContext.UserId));

        var swimlaneBoardId = await dbContext.Swimlanes
            .AsNoTracking()
            .Where(x => x.Id == command.SwimlaneId && !x.IsDeleted)
            .Select(x => new { x.BoardId })
            .SingleOrDefaultAsync(cancellationToken);
        if (swimlaneBoardId == null)
            return Result.Failure<int>(SwimlaneErrors.NotFound(command.SwimlaneId));
        Result<string> rankResult = await rankService.GenerateRankAsync(
            command.SwimlaneId, null, command.BeforeId, cancellationToken);
        if (!rankResult.IsSuccess)
            return Result.Failure<int>(rankResult.Error);

        var list = new List
        {
            BoardId = swimlaneBoardId.BoardId,
            SwimlaneId = command.SwimlaneId,
            Title = command.Title,
            Rank = rankResult.Value,
            Width = command.Width,
            CreatedById = userContext.UserId,
            CreatedAt = timeProvider.GetUtcNow(),
        };

        list.Raise((entity) =>
            new ListCreatedDomainEvent(entity.Id, entity.BoardId, entity.SwimlaneId, entity.Title,
                entity.Width, entity.Rank, userContext.ConnectionId));

        await dbContext.Lists.AddAsync(list, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return list.Id;
    }
}