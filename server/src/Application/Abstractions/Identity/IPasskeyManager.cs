using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Abstractions.Identity;

public interface IPasskeyManager
{
    Task<Result<PasskeyChallenge>> CreateRegistrationOptionsAsync(IUser user);

    Task<Result<PasskeyDetails>> RegisterAsync(IUser user, string credentialJson, string state, string? name);

    Task<IReadOnlyList<PasskeyDetails>> GetPasskeysAsync(IUser user);

    Task<Result> RenameAsync(IUser user, string passkeyId, string name);

    Task<Result> RemoveAsync(IUser user, string passkeyId);
}
