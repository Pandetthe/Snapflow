using Snapflow.Common;

namespace Snapflow.Domain.BoardMembers;

public static class BoardMemberErrors
{
    public static Error NotFound(int userId, int boardId) => Error.NotFound(
        "BoardMembers.NotFound",
        $"The user with the Id = '{userId}' is not a member of the board with Id = '{boardId}'.");
}
