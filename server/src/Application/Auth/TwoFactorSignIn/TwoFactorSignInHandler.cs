using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;

namespace Snapflow.Application.Auth.TwoFactorSignIn;

internal sealed class TwoFactorSignInHandler(ISignInManager signInManager) : ICommandHandler<TwoFactorSignInCommand>
{
    public Task<Result> Handle(TwoFactorSignInCommand command, CancellationToken cancellationToken = default) =>
        signInManager.TwoFactorSignInAsync(
            command.Code,
            command.RecoveryCode,
            command.PasskeyCredential,
            command.PasskeyState,
            command.RememberDevice,
            command.TwoFactorToken,
            command.UseCookies,
            command.UseSessionCookies);
}
