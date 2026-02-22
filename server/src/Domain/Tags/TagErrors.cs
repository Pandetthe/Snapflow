using Snapflow.Common;

namespace Snapflow.Domain.Tags;

public static class TagErrors
{
    public static Error NotFound(int tagId) => Error.NotFound(
        "Tags.NotFound",
        $"The tag with the Id = '{tagId}' was not found.");
}
