using Snapflow.Common;

namespace Snapflow.Domain.Roles;

public static class RoleErrors
{
    public static Error NotFound(string role) => Error.NotFound(
        "Roles.NotFound",
        $"The role '{role}' does not exist.");

    public static Error AlreadyAssigned(string role) => Error.Conflict(
        "Roles.AlreadyAssigned",
        $"The user already has the role '{role}'.");

    public static Error NotAssigned(string role) => Error.Problem(
        "Roles.NotAssigned",
        $"The user does not have the role '{role}'.");
}
