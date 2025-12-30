using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Cards.Update;

internal sealed class CreateCardCommandHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<UpdateCardCommand>
{
    public async Task<Result> Handle(UpdateCardCommand command, CancellationToken cancellationToken = default)
    {
        var userExists = await dbContext.Users.AsNoTracking()
            .AnyAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (!userExists)
            return Result.Failure<int>(UserErrors.NotFound(userContext.UserId));

        var card = await dbContext.Cards
            .SingleOrDefaultAsync(c => c.Id == command.Id && !c.IsDeleted, cancellationToken);
        if (card == null)
            return Result.Failure<int>(CardErrors.NotFound(command.Id));

        card.IsDeleted = true;
            card.DeletedAt = timeProvider.GetUtcNow();
        card.DeletedById = userContext.UserId;
        card.DeletedByCascade = false;

        card.Raise(new CardUpdatedDomainEvent(card.Id, card.BoardId, card.Title, card.Description));

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}