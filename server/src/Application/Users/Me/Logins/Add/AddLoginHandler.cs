using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.Logins.Add;

internal sealed class AddLoginHandler(
    IUserContext userContext,
    IUserManager userManager,
    ISignInManager signInManager) : ICommandHandler<AddLoginCommand, string>
{
    public async Task<Result<string>> Handle(AddLoginCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!userContext.IsAuthenticated)
                return Result.Failure<string>(UserErrors.Unauthorized);

            IUser user = await userContext.GetUserAsync();

            ExternalIdentity? identity = await signInManager.GetExternalLinkAsync(user);
            if (identity is null)
                return Result.Failure<string>(ExternalLoginErrors.LinkFailed);

            IUser? owner = await userManager.FindByLoginAsync(identity.Provider, identity.ProviderKey);
            if (owner is not null)
            {
                return owner.Id == user.Id
                    ? Result.Success(identity.Provider)
                    : Result.Failure<string>(ExternalLoginErrors.LinkedToAnotherAccount);
            }

            IReadOnlyList<ExternalLoginDetails> logins = await userManager.GetLoginsAsync(user);
            if (logins.Any(l => string.Equals(l.Provider, identity.Provider, StringComparison.Ordinal)))
                return Result.Failure<string>(ExternalLoginErrors.ProviderAlreadyLinked);

            Result linked = await userManager.AddLoginAsync(user, identity);
            if (linked.IsFailure)
                return Result.Failure<string>(linked.Error);

            await signInManager.RefreshSignInAsync(user);

            return Result.Success(identity.Provider);
        }
        finally
        {
            await signInManager.SignOutExternalAsync();
        }
    }
}
