using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

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

        if (policyName.StartsWith("Board:", StringComparison.Ordinal))
        {
            var parts = policyName.Split(':');
            var boardPermission = parts[1];

            permissionPolicy = new AuthorizationPolicyBuilder()
                .AddRequirements(new BoardPermissionRequirement(boardPermission))
                .Build();
        }
        else
        {
            permissionPolicy = new AuthorizationPolicyBuilder()
                .AddRequirements(new UserPermissionRequirement(policyName))
                .Build();
        }

        _authorizationOptions.AddPolicy(policyName, permissionPolicy);

        return permissionPolicy;
    }
}
