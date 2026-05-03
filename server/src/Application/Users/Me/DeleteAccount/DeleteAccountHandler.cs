using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.DeleteAccount;

internal sealed class DeleteAccountHandler(
    IUserContext userContext,
    IUserManager userManager,
    ISignInManager signInManager,
    TimeProvider timeProvider) : ICommandHandler<DeleteAccountCommand>
{
    public async Task<Result> Handle(DeleteAccountCommand command, CancellationToken cancellationToken = default)
    {
        IUser user = await userContext.GetUserAsync();
        Result result = await userManager.SoftDeleteAsync(user, timeProvider);
        if (!result.IsSuccess)
            return result;

        await signInManager.SignOutAsync();
        return Result.Success();
    }
}
