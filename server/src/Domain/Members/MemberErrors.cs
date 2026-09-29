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

    public static readonly Error CannotChangeOwnerRole = Error.Conflict(
        "Members.CannotChangeOwnerRole",
        "The board owner's role cannot be changed. Transfer ownership instead.");

    public static Error AlreadyOwner(int userId, int boardId) => Error.Conflict(
        "Members.AlreadyOwner",
        $"The user with Id = '{userId}' already owns the board with Id = '{boardId}'.");

    public static readonly Error CannotAssignOwner = Error.Problem(
        "Members.CannotAssignOwner",
        "The owner role cannot be assigned directly. Transfer ownership instead.");

    public static readonly Error DuplicateMember = Error.Conflict(
        "Members.Duplicate",
        "A user can be a member of the board only once.");

    public static Error AlreadyMember(int userId, int boardId) => Error.Conflict(
        "Members.AlreadyMember",
        $"The user with Id = '{userId}' is already a member of the board with Id = '{boardId}'.");
}
