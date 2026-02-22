using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Members;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Boards.Create;

internal sealed class CreateBoardHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider) : ICommandHandler<CreateBoardCommand, int>
{
    public async Task<Result<int>> Handle(CreateBoardCommand command, CancellationToken cancellationToken = default)
    {
        var userExists = await dbContext.Users.AsNoTracking()
            .AnyAsync(u => u.Id == userContext.UserId, cancellationToken);
        if (!userExists)
            return Result.Failure<int>(UserErrors.NotFound(userContext.UserId));

        var board = new Board
        {
            Title = command.Title,
            Description = command.Description,
            CreatedById = userContext.UserId,
            CreatedAt = timeProvider.GetUtcNow(),
        };

        board.Members.Add(new Member
        {
            Role = MemberRole.Owner,
            UserId = userContext.UserId,
        });

        await dbContext.Boards.AddAsync(board, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(board.Id);
    }
}
