using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Users;
using static Snapflow.Application.Cards.Update.UpdateCardResponse;

namespace Snapflow.Application.Cards.Update;

internal sealed class UpdateCardHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<UpdateCardCommand, UpdateCardResponse>
{
    public async Task<Result<UpdateCardResponse>> Handle(UpdateCardCommand command, CancellationToken cancellationToken = default)
    {
        var user = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (user == null)
            return Result.Failure<UpdateCardResponse>(UserErrors.NotFound(userContext.UserId));

        var card = await dbContext.Cards
            .SingleOrDefaultAsync(c => c.Id == command.Id && !c.IsDeleted, cancellationToken);
        if (card == null)
            return Result.Failure<UpdateCardResponse>(CardErrors.NotFound(command.Id));

        var updatedAt = timeProvider.GetUtcNow();

        card.Update(
            command.Title,
            command.Description,
            userContext.UserId,
            updatedAt,
            userContext.ConnectionId);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new UpdateCardResponse(
            updatedAt,
            UserDto.From(user));
    }
}