namespace Snapflow.Domain.Roles;

// Application-wide roles, independent of any board membership. Each one is seeded into the roles
// table, so adding a role here also needs an entry in AppRoleConfiguration and a migration.
public static class SystemRoles
{
    public const string Admin = "Admin";

    public static readonly IReadOnlyList<string> All = [Admin];
}
