using Snapflow.Common;

namespace Snapflow.Domain.Boards;

public static class BoardErrors
{
    public static Error NotFound(int boardId) => Error.NotFound(
        "Boards.NotFound",
        $"The board with the Id = '{boardId}' was not found.");

    public static Error VisibilityNotAllowed(BoardVisibility visibility) => Error.Problem(
        "Boards.VisibilityNotAllowed",
        $"The '{visibility}' visibility is not allowed on this server.");

    public static Error VisibilityChangeForbidden(int boardId) => Error.Forbidden(
        "Boards.VisibilityChangeForbidden",
        $"Only the owner can change the visibility of the board with the Id = '{boardId}'.");
}
