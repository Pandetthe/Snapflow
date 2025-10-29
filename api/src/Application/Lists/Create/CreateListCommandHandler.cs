using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
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
        IUser? user = await dbContext.Users.AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (user is null)
            return Result.Failure<int>(UserErrors.NotFound(userContext.UserId));
        var list = new List
        {
            SwimlaneId = command.SwimlaneId,
            Title = command.Title,
            CreatedById = user.Id,
            CreatedAt = timeProvider.GetUtcNow(),
        };
        await dbContext.Lists.AddAsync(list, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return list.Id;
    }
}