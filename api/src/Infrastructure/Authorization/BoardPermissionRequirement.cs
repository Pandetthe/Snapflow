using Microsoft.AspNetCore.Authorization;

namespace Snapflow.Infrastructure.Authorization;

internal sealed class BoardPermissionRequirement : IAuthorizationRequirement
{
    public BoardPermissionRequirement(string permission)
    {
        Permission = permission;
    }

    public string Permission { get; }
}
