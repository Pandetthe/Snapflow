namespace Snapflow.Domain.Roles;

public static class SystemRolePermissions
{
    private static readonly Dictionary<string, HashSet<string>> _permissionsByRole = new(StringComparer.Ordinal)
    {
        [SystemRoles.Admin] = new()
        {
            SystemPermissions.AdminPanel.Access,
            SystemPermissions.Users.View,
            SystemPermissions.Users.Update,
            SystemPermissions.Users.Delete,
            SystemPermissions.Roles.View,
            SystemPermissions.Roles.Assign,
            SystemPermissions.Boards.View,
            SystemPermissions.Boards.Delete
        },
    };

    // A user holding several roles gets the union of what each one grants. Unknown role names grant nothing.
    public static HashSet<string> For(IEnumerable<string> roles)
    {
        HashSet<string> permissions = new(StringComparer.Ordinal);

        foreach (string role in roles)
        {
            if (_permissionsByRole.TryGetValue(role, out HashSet<string>? granted))
                permissions.UnionWith(granted);
        }

        return permissions;
    }
}
