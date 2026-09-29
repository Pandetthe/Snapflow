using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Application.Abstractions.Ranking;
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
    IEntityRankService<Card> rankService,
    IAvatarService avatarService) : ICommandHandler<CreateCardCommand, CreateCardResponse>
{
    public Task<Result<CreateCardResponse>> Handle(CreateCardCommand command, CancellationToken cancellationToken = default) =>
        dbContext.InTransactionAsync(() => ExecuteAsync(command, cancellationToken), cancellationToken);

    private async Task<Result<CreateCardResponse>> ExecuteAsync(CreateCardCommand command, CancellationToken cancellationToken)
    {
        string? userName = await dbContext.FindUserNameAsync(userContext.UserId, cancellationToken);
        if (userName == null)
            return UserErrors.NotFound(userContext.UserId);

        var list = await dbContext.Lists
            .AsNoTracking()
            .Where(x => x.Id == command.ListId && x.BoardId == command.BoardId)
            .Select(x => new { x.BoardId, x.SwimlaneId })
            .SingleOrDefaultAsync(cancellationToken);
        if (list == null)
            return ListErrors.NotFound(command.ListId);
        var rankResult = await rankService.GenerateRankAsync(
            command.ListId, null, command.BeforeId, cancellationToken);
        if (!rankResult.IsSuccess)
            return rankResult.Error;

        DateTimeOffset createdAt = timeProvider.GetUtcNow();

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
            new UserDto(userContext.UserId, userName, avatarService.GenerateAvatarUrl(userContext.UserId)));
    }
}