namespace Snapflow.Application.Abstractions.Identity;

// For handlers that need to branch on an application-wide permission of the current user,
// e.g. an admin reaching a board they are not a member of. Endpoints should prefer RequireAuthorization.
public interface ISystemPermissionService
{
    Task<bool> HasPermissionAsync(string permission, CancellationToken cancellationToken = default);

    Task<IReadOnlySet<string>> GetPermissionsAsync(CancellationToken cancellationToken = default);
}
