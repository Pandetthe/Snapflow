using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Tags;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Tags.RemoveFromCard;

internal sealed class RemoveTagFromCardHandler(
    IAppDbContext dbContext,
    IUserContext userContext) : ICommandHandler<RemoveTagFromCardCommand>
{
    public async Task<Result> Handle(RemoveTagFromCardCommand command, CancellationToken cancellationToken = default)
    {
        Card? card = await dbContext.Cards
            .IgnoreQueryFilters([ISoftDeletable.FilterName])
            .Include(c => c.Tags)
            .SingleOrDefaultAsync(c => c.Id == command.CardId && c.BoardId == command.BoardId && !c.IsDeleted, cancellationToken);
        if (card == null)
            return CardErrors.NotFound(command.CardId);

        // A soft deleted tag is still taken off, so a card cannot keep one nobody can see.
        Tag? tag = await dbContext.Tags
            .IgnoreQueryFilters([ISoftDeletable.FilterName])
            .SingleOrDefaultAsync(t => t.Id == command.TagId && t.BoardId == command.BoardId, cancellationToken);
        if (tag == null)
            return TagErrors.NotFound(command.TagId);

        Result removed = card.RemoveTag(tag, userContext.UserId, userContext.ConnectionId);
        if (removed.IsFailure)
            return removed;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
