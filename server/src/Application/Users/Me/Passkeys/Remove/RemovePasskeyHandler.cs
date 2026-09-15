using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.Passkeys.Remove;

internal sealed class RemovePasskeyHandler(
    IUserContext userContext,
    IPasskeyManager passkeyManager) : ICommandHandler<RemovePasskeyCommand>
{
    public async Task<Result> Handle(RemovePasskeyCommand command, CancellationToken cancellationToken = default)
    {
        IUser user = await userContext.GetUserAsync();
        return await passkeyManager.RemoveAsync(user, command.PasskeyId);
    }
}
