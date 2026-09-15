using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Roles;

namespace Snapflow.Infrastructure.Authorization;

internal sealed class PermissionAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    private readonly AuthorizationOptions _authorizationOptions;

    public PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
        : base(options)
    {
        _authorizationOptions = options.Value;
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        AuthorizationPolicy? policy = await base.GetPolicyAsync(policyName);
        if (policy is not null)
            return policy;

        AuthorizationPolicy permissionPolicy;

        if (policyName.StartsWith(BoardPermissions.StartingPoint, StringComparison.Ordinal))
        {
            permissionPolicy = new AuthorizationPolicyBuilder()
                .AddRequirements(new BoardPermissionRequirement(policyName))
                .Build();
        }
        else if (policyName.StartsWith(SystemPermissions.StartingPoint, StringComparison.Ordinal))
        {
            permissionPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new SystemPermissionRequirement(policyName))
                .Build();
        }
        else
        {
            return null;
        }

        _authorizationOptions.AddPolicy(policyName, permissionPolicy);

        return permissionPolicy;
    }
}
