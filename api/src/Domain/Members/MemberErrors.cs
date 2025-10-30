using Snapflow.Common;

namespace Snapflow.Domain.Members;

public static class MemberErrors
{
    public static Error NotFound(int userId, int boardId) => Error.NotFound(
        "Members.NotFound",
        $"The user with the Id = '{userId}' is not a member of the board with Id = '{boardId}'.");
}
