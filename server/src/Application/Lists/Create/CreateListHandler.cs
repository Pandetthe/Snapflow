using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Application.Ranking;
using Snapflow.Common;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Swimlanes;
using Snapflow.Domain.Users;
using static Snapflow.Application.Lists.Create.CreateListResponse;

namespace Snapflow.Application.Lists.Create;

internal sealed class CreateListHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider,
    IEntityRankService<List> rankService) : ICommandHandler<CreateListCommand, CreateListResponse>
{
    public async Task<Result<CreateListResponse>> Handle(CreateListCommand command, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (user == null)
            return Result.Failure<CreateListResponse>(UserErrors.NotFound(userContext.UserId));

        var swimlaneBoardId = await dbContext.Swimlanes
            .AsNoTracking()
            .Where(x => x.Id == command.SwimlaneId && !x.IsDeleted)
            .Select(x => new { x.BoardId })
            .SingleOrDefaultAsync(cancellationToken);
        if (swimlaneBoardId == null)
            return Result.Failure<CreateListResponse>(SwimlaneErrors.NotFound(command.SwimlaneId));
        Result<string> rankResult = await rankService.GenerateRankAsync(
            command.SwimlaneId, null, command.BeforeId, cancellationToken);
        if (!rankResult.IsSuccess)
            return Result.Failure<CreateListResponse>(rankResult.Error);

        var createdAt = timeProvider.GetUtcNow();

        var list = List.Create(
            swimlaneBoardId.BoardId,
            command.SwimlaneId,
            command.Title,
            command.Width,
            rankResult.Value,
            userContext.UserId,
            createdAt,
            userContext.ConnectionId);

        await dbContext.Lists.AddAsync(list, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateListResponse(
            list.Id,
            Rank: list.Rank,
            createdAt,
            UserDto.From(user));
    }
}