using Microsoft.AspNetCore.Authorization;

namespace Snapflow.Infrastructure.Authorization;

internal sealed class SystemPermissionRequirement : IAuthorizationRequirement
{
    public SystemPermissionRequirement(string permission)
    {
        Permission = permission;
    }

    public string Permission { get; }
}
