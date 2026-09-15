using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;

namespace Snapflow.Application.Auth.PasskeySignInOptions;

internal sealed class PasskeySignInOptionsHandler(ISignInManager signInManager)
    : ICommandHandler<PasskeySignInOptionsCommand, PasskeyChallenge>
{
    public async Task<Result<PasskeyChallenge>> Handle(PasskeySignInOptionsCommand command, CancellationToken cancellationToken = default) =>
        await signInManager.CreatePasskeySignInOptionsAsync();
}
