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
        // The tags already on the card are loaded so the join row is not inserted twice.
        Card? card = await dbContext.Cards
            .Include(c => c.Tags)
            .SingleOrDefaultAsync(c => c.Id == command.CardId && c.BoardId == command.BoardId, cancellationToken);
        if (card == null)
            return CardErrors.NotFound(command.CardId);

        Tag? tag = await dbContext.Tags
            .SingleOrDefaultAsync(t => t.Id == command.TagId && t.BoardId == command.BoardId, cancellationToken);
        if (tag == null)
            return TagErrors.NotFound(command.TagId);

        Result added = card.AddTag(tag, userContext.UserId, userContext.ConnectionId);
        if (added.IsFailure)
            return added;

        return await dbContext.TrySaveChangesAsync(
            [new UniqueConflict(DbConstraints.CardTagKey, TagErrors.AlreadyOnCard(command.TagId, command.CardId))],
            cancellationToken);
    }
}
