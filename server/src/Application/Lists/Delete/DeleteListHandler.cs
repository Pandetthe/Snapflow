using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Lists.Delete;

internal sealed class DeleteListHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<DeleteListCommand>
{
    public async Task<Result> Handle(DeleteListCommand command, CancellationToken cancellationToken = default)
    {
        var userExists = await dbContext.Users.AsNoTracking()
            .AnyAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (!userExists)
            return Result.Failure(UserErrors.NotFound(userContext.UserId));

        var list = await dbContext.Lists
            .SingleOrDefaultAsync(l => l.Id == command.Id && !l.IsDeleted, cancellationToken);
        if (list == null)
            return Result.Failure(ListErrors.NotFound(command.Id));

        DateTimeOffset dateTimeOffset = timeProvider.GetUtcNow();
        int userId = userContext.UserId;

        using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            list.SoftDelete(userId, dateTimeOffset, userContext.ConnectionId);

            await dbContext.Cards
                .Where(c => c.ListId == list.Id && !c.IsDeleted)
                .ExecuteUpdateAsync(c => c
                    .SetProperty(x => x.IsDeleted, true)
                    .SetProperty(x => x.DeletedAt, dateTimeOffset)
                    .SetProperty(x => x.DeletedById, userId)
                    .SetProperty(x => x.DeletedByCascade, true),
                    cancellationToken);

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        return Result.Success();
    }
}