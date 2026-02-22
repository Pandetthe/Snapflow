using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Cards.Update;

internal sealed class UpdateCardHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<UpdateCardCommand>
{
    public async Task<Result> Handle(UpdateCardCommand command, CancellationToken cancellationToken = default)
    {
        var userExists = await dbContext.Users.AsNoTracking()
            .AnyAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (!userExists)
            return Result.Failure(UserErrors.NotFound(userContext.UserId));

        var card = await dbContext.Cards
            .SingleOrDefaultAsync(c => c.Id == command.Id && !c.IsDeleted, cancellationToken);
        if (card == null)
            return Result.Failure(CardErrors.NotFound(command.Id));

        card.Update(
            command.Title,
            command.Description,
            userContext.UserId,
            timeProvider.GetUtcNow(),
            userContext.ConnectionId);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}