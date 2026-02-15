using Snapflow.Common;

namespace Snapflow.Domain.Lists;

public static class ListErrors
{
    public static Error NotFound(int listId) => Error.NotFound(
        "Lists.NotFound",
        $"The list with the Id = '{listId}' was not found.");
}
