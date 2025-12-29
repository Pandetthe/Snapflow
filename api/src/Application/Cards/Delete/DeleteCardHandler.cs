using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Cards.Delete;

internal sealed class DeleteCardHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<DeleteCardCommand>
{
    public async Task<Result> Handle(DeleteCardCommand command, CancellationToken cancellationToken = default)
    {
        var userExists = await dbContext.Users.AsNoTracking()
            .AnyAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (!userExists)
            return Result.Failure(UserErrors.NotFound(userContext.UserId));

        var card = await dbContext.Cards
            .SingleOrDefaultAsync(c => c.Id == command.Id && !c.IsDeleted, cancellationToken);
        if (card == null)
            return Result.Failure(CardErrors.NotFound(command.Id));

        DateTimeOffset dateTimeOffset = timeProvider.GetUtcNow();
        int userId = userContext.UserId;

        card.IsDeleted = true;
        card.DeletedById = userId;
        card.DeletedAt = dateTimeOffset;
        card.DeletedByCascade = false;


        card.Raise(new CardDeletedDomainEvent(card.Id, card.BoardId));

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}