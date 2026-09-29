using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Application.Abstractions.Ranking;
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
    public Task<Result<CreateListResponse>> Handle(CreateListCommand command, CancellationToken cancellationToken = default) =>
        dbContext.InTransactionAsync(() => ExecuteAsync(command, cancellationToken), cancellationToken);

    private async Task<Result<CreateListResponse>> ExecuteAsync(CreateListCommand command, CancellationToken cancellationToken)
    {
        IUser? user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (user == null)
            return UserErrors.NotFound(userContext.UserId);

        var swimlaneBoardId = await dbContext.Swimlanes
            .AsNoTracking()
            .Where(x => x.Id == command.SwimlaneId && x.BoardId == command.BoardId)
            .Select(x => new { x.BoardId })
            .SingleOrDefaultAsync(cancellationToken);
        if (swimlaneBoardId == null)
            return SwimlaneErrors.NotFound(command.SwimlaneId);
        var rankResult = await rankService.GenerateRankAsync(
            command.SwimlaneId, null, command.BeforeId, cancellationToken);
        if (!rankResult.IsSuccess)
            return rankResult.Error;

        DateTimeOffset createdAt = timeProvider.GetUtcNow();

        var list = List.Create(
            swimlaneBoardId.BoardId,
            command.SwimlaneId,
            command.Title,
            command.Width,
            rankResult.Value,
            user,
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