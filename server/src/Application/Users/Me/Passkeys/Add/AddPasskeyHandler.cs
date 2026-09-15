using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.Passkeys.Add;

internal sealed class AddPasskeyHandler(
    IUserContext userContext,
    IPasskeyManager passkeyManager) : ICommandHandler<AddPasskeyCommand, PasskeyDetails>
{
    public async Task<Result<PasskeyDetails>> Handle(AddPasskeyCommand command, CancellationToken cancellationToken = default)
    {
        IUser user = await userContext.GetUserAsync();
        return await passkeyManager.RegisterAsync(user, command.Credential, command.State, command.Name);
    }
}
