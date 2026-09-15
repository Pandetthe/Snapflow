using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Tags;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Tags.AddToCard;

internal sealed class AddTagToCardHandler(
    IAppDbContext dbContext,
    IUserContext userContext) : ICommandHandler<AddTagToCardCommand>
{
    public async Task<Result> Handle(AddTagToCardCommand command, CancellationToken cancellationToken = default)
    {
        IUser? user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (user == null)
            return Result.Failure(UserErrors.NotFound(userContext.UserId));

        // The tags already on the card are loaded so the join row is not inserted twice.
        Card? card = await dbContext.Cards
            .Include(c => c.Tags)
            .SingleOrDefaultAsync(c => c.Id == command.CardId && c.BoardId == command.BoardId && !c.IsDeleted, cancellationToken);
        if (card == null)
            return Result.Failure(CardErrors.NotFound(command.CardId));

        Tag? tag = await dbContext.Tags
            .SingleOrDefaultAsync(t => t.Id == command.TagId && t.BoardId == command.BoardId && !t.IsDeleted, cancellationToken);
        if (tag == null)
            return Result.Failure(TagErrors.NotFound(command.TagId));

        if (!card.AddTag(tag, user, userContext.ConnectionId))
            return Result.Failure(TagErrors.AlreadyOnCard(command.TagId, command.CardId));

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
