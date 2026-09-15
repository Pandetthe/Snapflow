namespace Snapflow.Domain.Roles;

// Application-wide permissions, granted through SystemRoles rather than through a board membership.
// Pass one to RequireAuthorization the same way as a BoardPermissions value.
public static class SystemPermissions
{
    public const string Separator = ":";
    public const string Base = "System";
    public const string StartingPoint = Base + Separator;

    public static class AdminPanel
    {
        public const string Base = SystemPermissions.StartingPoint + "AdminPanel";
        public const string StartingPoint = Base + Separator;

        public const string Access = StartingPoint + "Access";
    }

    public static class Users
    {
        public const string Base = SystemPermissions.StartingPoint + "User";
        public const string StartingPoint = Base + Separator;

        public const string View = StartingPoint + "View";
        public const string Update = StartingPoint + "Update";
        public const string Delete = StartingPoint + "Delete";
    }

    public static class Roles
    {
        public const string Base = SystemPermissions.StartingPoint + "Role";
        public const string StartingPoint = Base + Separator;

        public const string View = StartingPoint + "View";
        public const string Assign = StartingPoint + "Assign";
    }

    // Reaching any board, whether or not the user is a member of it.
    public static class Boards
    {
        public const string Base = SystemPermissions.StartingPoint + "Board";
        public const string StartingPoint = Base + Separator;

        public const string View = StartingPoint + "View";
        public const string Delete = StartingPoint + "Delete";
    }
}
