using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.Logins.Remove;

internal sealed class RemoveLoginHandler(
    IUserContext userContext,
    IUserManager userManager,
    IPasskeyManager passkeyManager,
    ISignInManager signInManager,
    IAuthenticationSettings settings) : ICommandHandler<RemoveLoginCommand>
{
    public async Task<Result> Handle(RemoveLoginCommand command, CancellationToken cancellationToken = default)
    {
        IUser user = await userContext.GetUserAsync();

        IReadOnlyList<ExternalLoginDetails> logins = await userManager.GetLoginsAsync(user);
        if (!logins.Any(l => string.Equals(l.Provider, command.Provider, StringComparison.Ordinal)))
            return Result.Failure(ExternalLoginErrors.NotFound);

        if (!await HasOtherSignInMethodAsync(user, logins, command.Provider))
            return Result.Failure(ExternalLoginErrors.LastSignInMethod);

        Result removed = await userManager.RemoveLoginAsync(user, command.Provider);
        if (removed.IsFailure)
            return removed;

        await signInManager.RefreshSignInAsync(user);

        return Result.Success();
    }

    private async Task<bool> HasOtherSignInMethodAsync(IUser user, IReadOnlyList<ExternalLoginDetails> logins, string provider)
    {
        if (logins.Any(l => !string.Equals(l.Provider, provider, StringComparison.Ordinal) && settings.IsLoginProviderAvailable(l.Provider)))
            return true;

        if (!settings.PasswordAuthenticationEnabled)
            return false;

        return await userManager.HasPasswordAsync(user)
            || (await passkeyManager.GetPasskeysAsync(user)).Count > 0;
    }
}
