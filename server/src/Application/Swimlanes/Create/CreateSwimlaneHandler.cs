using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Application.Abstractions.Ranking;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Swimlanes;
using Snapflow.Domain.Users;
using static Snapflow.Application.Swimlanes.Create.CreateSwimlaneResponse;

namespace Snapflow.Application.Swimlanes.Create;

internal sealed class CreateSwimlaneHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider,
    IEntityRankService<Swimlane> rankService) : ICommandHandler<CreateSwimlaneCommand, CreateSwimlaneResponse>
{
    public Task<Result<CreateSwimlaneResponse>> Handle(CreateSwimlaneCommand command, CancellationToken cancellationToken = default) =>
        dbContext.InTransactionAsync(() => ExecuteAsync(command, cancellationToken), cancellationToken);

    private async Task<Result<CreateSwimlaneResponse>> ExecuteAsync(CreateSwimlaneCommand command, CancellationToken cancellationToken)
    {
        string? userName = await dbContext.FindUserNameAsync(userContext.UserId, cancellationToken);
        if (userName == null)
            return UserErrors.NotFound(userContext.UserId);

        var boardExists = await dbContext.Boards.AsNoTracking()
            .AnyAsync(b => b.Id == command.BoardId, cancellationToken);
        if (!boardExists)
            return BoardErrors.NotFound(command.BoardId);
        var rankResult = await rankService.GenerateRankAsync(
            command.BoardId, null, command.BeforeId, cancellationToken);
        if (!rankResult.IsSuccess)
            return rankResult.Error;

        DateTimeOffset createdAt = timeProvider.GetUtcNow();

        var swimlane = Swimlane.Create(
            command.BoardId,
            command.Title,
            command.Height,
            rankResult.Value,
            userContext.UserId,
            createdAt,
            userContext.ConnectionId);

        await dbContext.Swimlanes.AddAsync(swimlane, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateSwimlaneResponse(
            swimlane.Id,
            swimlane.Rank,
            createdAt,
            new UserDto(userContext.UserId, userName));
    }
}