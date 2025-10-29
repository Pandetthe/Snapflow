using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Cards;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Cards.Create;

internal sealed class CreateCardCommandHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<CreateCardCommand, int>
{
    public async Task<Result<int>> Handle(CreateCardCommand command, CancellationToken cancellationToken = default)
    {
        IUser? user = await dbContext.Users.AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (user is null)
            return Result.Failure<int>(UserErrors.NotFound(userContext.UserId));
        var card = new Card
        {
            ListId = command.ListId,
            Title = command.Title,
            Description = command.Description,
            CreatedById = user.Id,
            CreatedAt = timeProvider.GetUtcNow(),
        };
        await dbContext.Cards.AddAsync(card, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return card.Id;
    }
}