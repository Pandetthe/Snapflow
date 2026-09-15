using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.Passkeys.Rename;

internal sealed class RenamePasskeyHandler(
    IUserContext userContext,
    IPasskeyManager passkeyManager) : ICommandHandler<RenamePasskeyCommand>
{
    public async Task<Result> Handle(RenamePasskeyCommand command, CancellationToken cancellationToken = default)
    {
        IUser user = await userContext.GetUserAsync();
        return await passkeyManager.RenameAsync(user, command.PasskeyId, command.Name);
    }
}
