using Snapflow.Common;

namespace Snapflow.Domain.Members;

public static class MemberErrors
{
    public static Error NotFound(int userId, int boardId) => Error.NotFound(
        "Members.NotFound",
        $"The user with the Id = '{userId}' is not a member of the board with Id = '{boardId}'.");

    public static Error OwnerAlreadyExists(int boardId) => Error.Conflict(
        "Members.OwnerAlreadyExists",
        $"The board with Id = '{boardId}' already has an owner.");

    public static readonly Error CannotRemoveOwner = Error.Conflict(
        "Members.CannotRemoveOwner",
        "The board owner cannot be removed. Transfer ownership first.");

    public static Error AlreadyMember(int userId, int boardId) => Error.Conflict(
        "Members.AlreadyMember",
        $"The user with Id = '{userId}' is already a member of the board with Id = '{boardId}'.");
}
