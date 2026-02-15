using Snapflow.Common;

namespace Snapflow.Domain.Swimlanes;

public static class SwimlaneErrors
{
    public static Error NotFound(int swimlaneId) => Error.NotFound(
        "Swimlanes.NotFound",
        $"The swimlane with the Id = '{swimlaneId}' was not found.");
}
