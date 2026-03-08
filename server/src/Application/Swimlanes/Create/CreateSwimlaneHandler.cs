using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Application.Ranking;
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
    public async Task<Result<CreateSwimlaneResponse>> Handle(CreateSwimlaneCommand command, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (user == null)
            return Result.Failure<CreateSwimlaneResponse>(UserErrors.NotFound(userContext.UserId));

        var boardExists = await dbContext.Boards.AsNoTracking()
            .AnyAsync(b => b.Id == command.BoardId && !b.IsDeleted, cancellationToken);
        if (!boardExists)
            return Result.Failure<CreateSwimlaneResponse>(BoardErrors.NotFound(command.BoardId));
        Result<string> rankResult = await rankService.GenerateRankAsync(
            command.BoardId, null, command.BeforeId, cancellationToken);
        if (!rankResult.IsSuccess)
            return Result.Failure<CreateSwimlaneResponse>(rankResult.Error);

        var createdAt = timeProvider.GetUtcNow();

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
            UserDto.From(user));
    }
}