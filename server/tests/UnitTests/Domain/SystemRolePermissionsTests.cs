using System.Reflection;
using FluentAssertions;
using Snapflow.Domain.Roles;

namespace Snapflow.UnitTests.Domain;

public sealed class SystemRolePermissionsTests
{
    [Fact]
    public void For_Should_GrantEveryPermission_When_Admin()
    {
        var permissions = SystemRolePermissions.For([SystemRoles.Admin]);

        permissions.Should().BeEquivalentTo(AllPermissions());
    }

    [Fact]
    public void For_Should_GrantNothing_When_NoRoles()
    {
        var permissions = SystemRolePermissions.For([]);

        permissions.Should().BeEmpty();
    }

    [Fact]
    public void For_Should_IgnoreRole_When_Unknown()
    {
        var permissions = SystemRolePermissions.For(["NotARole"]);

        permissions.Should().BeEmpty();
    }

    [Fact]
    public void Permissions_Should_AllStartWithSystemPrefix()
    {
        AllPermissions().Should().OnlyContain(p => p.StartsWith(SystemPermissions.StartingPoint, StringComparison.Ordinal));
    }

    // Leaf constants only; the Base/StartingPoint/Separator helpers are prefixes, not permissions.
    private static IEnumerable<string> AllPermissions() =>
        typeof(SystemPermissions).GetNestedTypes()
            .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static))
            .Where(f => f.IsLiteral && f.Name is not ("Base" or "StartingPoint"))
            .Select(f => (string)f.GetRawConstantValue()!);
}
