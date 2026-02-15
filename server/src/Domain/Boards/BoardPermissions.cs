namespace Snapflow.Domain.Boards;

public static class BoardPermissions
{
    public const string Separator = ":";
    public const string Base = "Board";
    public const string StartingPoint = Base + Separator;

    public static class Boards
    {
        public const string Base = BoardPermissions.Base;
        public const string StartingPoint = BoardPermissions.StartingPoint;

        public const string View = StartingPoint + "View";
        public const string Update = StartingPoint + "Update";
        public const string Delete = StartingPoint + "Delete";
    }

    public static class Swimlanes
    {
        public const string Base = BoardPermissions.StartingPoint + "Swimlane";
        public const string StartingPoint = Base + Separator;

        public const string Create = StartingPoint + "Create";
        public const string Update = StartingPoint + "Update";
        public const string Delete = StartingPoint + "Delete";
        public const string Move = StartingPoint + "Move";
    }

    public static class Lists
    {
        public const string Base = BoardPermissions.StartingPoint + "List";
        public const string StartingPoint = Base + Separator;

        public const string Create = StartingPoint + "Create";
        public const string Update = StartingPoint + "Update";
        public const string Delete = StartingPoint + "Delete";
        public const string Move = StartingPoint + "Move";
    }

    public static class Cards
    {
        public const string Base = BoardPermissions.StartingPoint + "Card";
        public const string StartingPoint = Base + Separator;

        public const string Create = StartingPoint + "Create";
        public const string Update = StartingPoint + "Update";
        public const string Delete = StartingPoint + "Delete";
        public const string Move = StartingPoint + "Move";
    }
}
