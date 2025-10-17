using Microsoft.AspNetCore.Authorization;

namespace Snapflow.Infrastructure.Authorization;

internal sealed class UserPermissionRequirement : IAuthorizationRequirement
{
    public UserPermissionRequirement(string permission)
    {
        Permission = permission;
    }

    public string Permission { get; }
}
