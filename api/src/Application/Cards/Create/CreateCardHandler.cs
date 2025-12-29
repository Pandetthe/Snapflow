using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Behaviours;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Application.Ranking;
using Snapflow.Common;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Cards.Create;

internal sealed class CreateCardHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider,
    IEntityRankService<Card> rankService) : ICommandHandler<CreateCardCommand, int>
{
    public async Task<Result<int>> Handle(CreateCardCommand command, CancellationToken cancellationToken = default)
    {
        var userExists = await dbContext.Users.AsNoTracking()
            .AnyAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (!userExists)
            return Result.Failure<int>(UserErrors.NotFound(userContext.UserId));

        var list = await dbContext.Lists
            .AsNoTracking()
            .Where(x => x.Id == command.ListId && !x.IsDeleted)
            .Select(x => new { x.BoardId, x.SwimlaneId })
            .SingleOrDefaultAsync(cancellationToken);
        if (list == null)
            return Result.Failure<int>(ListErrors.NotFound(command.ListId));
        Result<string> rankResult = await rankService.GenerateRankAsync(
            command.ListId, null, command.BeforeId, cancellationToken);
        if (!rankResult.IsSuccess)
            return Result.Failure<int>(rankResult.Error);

        var card = new Card
        {
            ListId = command.ListId,
            SwimlaneId = list.SwimlaneId,
            BoardId = list.BoardId,
            Title = command.Title,
            Description = command.Description,
            Rank = rankResult.Value,
            CreatedById = userContext.UserId,
            CreatedAt = timeProvider.GetUtcNow(),
        };

        card.Raise(new CardCreatedDomainEvent(card.BoardId, card.SwimlaneId, card.ListId,
            card.Title, card.Description));

        await dbContext.Cards.AddAsync(card, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return card.Id;
    }
}