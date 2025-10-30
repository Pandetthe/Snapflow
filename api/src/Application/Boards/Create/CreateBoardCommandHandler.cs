using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Members;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Boards.Create;

internal sealed class CreateBoardCommandHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider)
    : ICommandHandler<CreateBoardCommand, int>
{
    public async Task<Result<int>> Handle(CreateBoardCommand command, CancellationToken cancellationToken = default)
    {
        IUser? user = await dbContext.Users.AsNoTracking()
            .SingleOrDefaultAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (user is null)
            return Result.Failure<int>(UserErrors.NotFound(userContext.UserId));
        var board = new Board
        {
            Title = command.Title,
            Description = command.Description,
            CreatedById = user.Id,
            CreatedAt = timeProvider.GetUtcNow(),
            Members = [
                new Member
                {
                    Role = MemberRole.Owner,
                    UserId = user.Id,
                }
            ]
        };
        await dbContext.Boards.AddAsync(board, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return board.Id;
    }
}
