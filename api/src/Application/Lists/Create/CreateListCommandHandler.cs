using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Lists.Create;

internal sealed class CreateListCommandHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<CreateListCommand, int>
{
    public async Task<Result<int>> Handle(CreateListCommand command, CancellationToken cancellationToken = default)
    {
        var userExists = await dbContext.Users.AsNoTracking()
            .AnyAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (!userExists)
            return Result.Failure<int>(UserErrors.NotFound(userContext.UserId));

        var boardExists = await dbContext.Boards.AsNoTracking()
            .AnyAsync(b => b.Id == command.BoardId, cancellationToken);
        if (!boardExists)
            return Result.Failure<int>(BoardErrors.NotFound(command.BoardId));

        var list = new List
        {
            BoardId = command.BoardId,
            SwimlaneId = command.SwimlaneId,
            Title = command.Title,
            CreatedById = userContext.UserId,
            CreatedAt = timeProvider.GetUtcNow(),
        };

        list.Raise(new ListCreatedDomainEvent(list.BoardId, list.SwimlaneId, list.Title));

        await dbContext.Lists.AddAsync(list, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return list.Id;
    }
}