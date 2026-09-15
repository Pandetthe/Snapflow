using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;

namespace Snapflow.Application.Auth.TwoFactorPasskeyOptions;

internal sealed class TwoFactorPasskeyOptionsHandler(ISignInManager signInManager)
    : ICommandHandler<TwoFactorPasskeyOptionsCommand, PasskeyChallenge>
{
    public Task<Result<PasskeyChallenge>> Handle(TwoFactorPasskeyOptionsCommand command, CancellationToken cancellationToken = default) =>
        signInManager.CreateTwoFactorPasskeyOptionsAsync(command.TwoFactorToken);
}
