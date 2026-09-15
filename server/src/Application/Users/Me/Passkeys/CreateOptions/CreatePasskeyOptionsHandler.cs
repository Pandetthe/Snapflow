using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.Passkeys.CreateOptions;

internal sealed class CreatePasskeyOptionsHandler(
    IUserContext userContext,
    IPasskeyManager passkeyManager) : ICommandHandler<CreatePasskeyOptionsCommand, PasskeyChallenge>
{
    public async Task<Result<PasskeyChallenge>> Handle(CreatePasskeyOptionsCommand command, CancellationToken cancellationToken = default)
    {
        IUser user = await userContext.GetUserAsync();
        return await passkeyManager.CreateRegistrationOptionsAsync(user);
    }
}
