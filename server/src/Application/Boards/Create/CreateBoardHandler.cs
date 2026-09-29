using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Members;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Boards.Create;

internal sealed class CreateBoardHandler(
    IAppDbContext dbContext,
    IUserContext userContext,
    IBoardVisibilityPolicy visibilityPolicy,
    TimeProvider timeProvider) : ICommandHandler<CreateBoardCommand, int>
{
    public async Task<Result<int>> Handle(CreateBoardCommand command, CancellationToken cancellationToken = default)
    {
        if (!visibilityPolicy.IsAllowed(command.Visibility))
            return BoardErrors.VisibilityNotAllowed(command.Visibility);

        var board = Board.Create(
            command.Title,
            command.Description,
            command.Visibility,
            userContext.UserId,
            timeProvider.GetUtcNow(),
            userContext.ConnectionId);

        if (command.Members?.Count > 0)
        {
            var memberUserIds = command.Members.Select(m => m.UserId).ToList();
            
            var existingUsers = await dbContext.Users.AsNoTracking()
                .Where(u => memberUserIds.Contains(u.Id))
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);
            
            var notFoundUserId = memberUserIds.FirstOrDefault(id => !existingUsers.Contains(id));
            if (notFoundUserId != 0)
                return UserErrors.NotFound(notFoundUserId);

            foreach (CreateBoardMemberRequest member in command.Members)
            {
                Result added = board.AddMember(member.UserId, member.Role, userContext.ConnectionId);
                if (added.IsFailure)
                    return added.Error;
            }
        }

        await dbContext.Boards.AddAsync(board, cancellationToken);

        Result saved = await dbContext.TrySaveChangesAsync(
            [new UniqueConflict(DbConstraints.BoardMemberKey, MemberErrors.DuplicateMember)],
            cancellationToken);
        if (saved.IsFailure)
            return saved.Error;

        return Result.Success(board.Id);
    }
}
