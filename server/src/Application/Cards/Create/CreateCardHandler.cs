using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Application.Ranking;
using Snapflow.Common;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Lists;
using Snapflow.Domain.Users;
using static Snapflow.Application.Cards.Create.CreateCardResponse;

namespace Snapflow.Application.Cards.Create;

internal sealed class CreateCardHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider,
    IEntityRankService<Card> rankService) : ICommandHandler<CreateCardCommand, CreateCardResponse>
{
    public async Task<Result<CreateCardResponse>> Handle(CreateCardCommand command, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (user == null)
            return Result.Failure<CreateCardResponse>(UserErrors.NotFound(userContext.UserId));

        var list = await dbContext.Lists
            .AsNoTracking()
            .Where(x => x.Id == command.ListId && !x.IsDeleted)
            .Select(x => new { x.BoardId, x.SwimlaneId })
            .SingleOrDefaultAsync(cancellationToken);
        if (list == null)
            return Result.Failure<CreateCardResponse>(ListErrors.NotFound(command.ListId));
        Result<string> rankResult = await rankService.GenerateRankAsync(
            command.ListId, null, command.BeforeId, cancellationToken);
        if (!rankResult.IsSuccess)
            return Result.Failure<CreateCardResponse>(rankResult.Error);

        var createdAt = timeProvider.GetUtcNow();

        var card = Card.Create(
            list.BoardId,
            list.SwimlaneId,
            command.ListId,
            command.Title,
            command.Description,
            rankResult.Value,
            userContext.UserId,
            createdAt,
            userContext.ConnectionId);

        await dbContext.Cards.AddAsync(card, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateCardResponse(
            card.Id,
            card.Rank,
            createdAt,
            UserDto.From(user));
    }
}