using Snapflow.Domain.Boards;

namespace Snapflow.Domain.Members;

public static class MemberRolePermissions
{
    private static readonly HashSet<string> _nonMember =
    [
        BoardPermissions.Boards.View
    ];

    private static readonly Dictionary<MemberRole, HashSet<string>> _permissionsByRole = new()
    {
        [MemberRole.Owner] = new()
        {
            BoardPermissions.Boards.View,
            BoardPermissions.Boards.Update,
            BoardPermissions.Boards.Delete,
            BoardPermissions.Swimlanes.Create,
            BoardPermissions.Swimlanes.Update,
            BoardPermissions.Swimlanes.Delete,
            BoardPermissions.Swimlanes.Move,
            BoardPermissions.Lists.Create,
            BoardPermissions.Lists.Update,
            BoardPermissions.Lists.Delete,
            BoardPermissions.Lists.Move,
            BoardPermissions.Cards.Create,
            BoardPermissions.Cards.Update,
            BoardPermissions.Cards.Delete,
            BoardPermissions.Cards.Move,
            BoardPermissions.Tags.Create,
            BoardPermissions.Tags.Update,
            BoardPermissions.Tags.Delete,
            BoardPermissions.Tags.Assign,
            BoardPermissions.Boards.Transfer,
            BoardPermissions.Boards.ChangeVisibility
        },
        [MemberRole.Admin] = new()
        {
            BoardPermissions.Boards.View,
            BoardPermissions.Boards.Update,
            BoardPermissions.Swimlanes.Create,
            BoardPermissions.Swimlanes.Update,
            BoardPermissions.Swimlanes.Delete,
            BoardPermissions.Swimlanes.Move,
            BoardPermissions.Lists.Create,
            BoardPermissions.Lists.Update,
            BoardPermissions.Lists.Delete,
            BoardPermissions.Lists.Move,
            BoardPermissions.Cards.Create,
            BoardPermissions.Cards.Update,
            BoardPermissions.Cards.Delete,
            BoardPermissions.Cards.Move,
            BoardPermissions.Tags.Create,
            BoardPermissions.Tags.Update,
            BoardPermissions.Tags.Delete,
            BoardPermissions.Tags.Assign
        },
        [MemberRole.Member] = new()
        {
            BoardPermissions.Boards.View,
            BoardPermissions.Cards.Create,
            BoardPermissions.Cards.Update,
            BoardPermissions.Cards.Delete,
            BoardPermissions.Cards.Move,
            BoardPermissions.Tags.Assign
        },
        [MemberRole.Viewer] = new()
        {
            BoardPermissions.Boards.View,
        },
    };

    public static IReadOnlySet<string> NonMember => _nonMember;

    public static IReadOnlySet<string> For(MemberRole role) =>
        _permissionsByRole.TryGetValue(role, out HashSet<string>? permissions) ? permissions : new HashSet<string>();
}
