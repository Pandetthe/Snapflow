using System.Reflection;
using Snapflow.Domain.Roles;

namespace Snapflow.UnitTests.Domain;

public sealed class SystemRolePermissionsTests
{
    [Fact]
    public void For_Should_GrantEveryPermission_When_Admin()
    {
        var permissions = SystemRolePermissions.For([SystemRoles.Admin]);

        Assert.Equal(AllPermissions().Order(), permissions.Order());
    }

    [Fact]
    public void For_Should_GrantNothing_When_NoRoles()
    {
        var permissions = SystemRolePermissions.For([]);

        Assert.Empty(permissions);
    }

    [Fact]
    public void For_Should_IgnoreRole_When_Unknown()
    {
        var permissions = SystemRolePermissions.For(["NotARole"]);

        Assert.Empty(permissions);
    }

    [Fact]
    public void Permissions_Should_AllStartWithSystemPrefix()
    {
        Assert.All(AllPermissions(), p => Assert.StartsWith(SystemPermissions.StartingPoint, p, StringComparison.Ordinal));
    }

    // Leaf constants only; the Base/StartingPoint/Separator helpers are prefixes, not permissions.
    private static IEnumerable<string> AllPermissions() =>
        typeof(SystemPermissions).GetNestedTypes()
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static))
            .Where(f => f.IsLiteral && f.Name is not ("Base" or "StartingPoint"))
            .Select(f => (string)f.GetRawConstantValue()!);
}
