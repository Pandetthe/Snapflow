using Snapflow.Common;

namespace Snapflow.Domain.Boards;

public static class BoardErrors
{
    public static Error NotFound(int boardId) => Error.NotFound(
        "Boards.NotFound",
        $"The board with the Id = '{boardId}' was not found");
}
