using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;

namespace Snapflow.Application.Boards.Update;

internal sealed class UpdateBoardHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<UpdateBoardCommand>
{
    public async Task<Result> Handle(UpdateBoardCommand command, CancellationToken cancellationToken = default)
    {
        Board? board = await dbContext.Boards
            .SingleOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (board == null)
            return BoardErrors.NotFound(command.Id);

        board.Update(
            command.Title ?? board.Title,
            command.Description ?? board.Description,
            userContext.UserId,
            timeProvider.GetUtcNow(),
            userContext.ConnectionId);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}