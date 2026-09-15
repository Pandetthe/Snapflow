using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;

namespace Snapflow.Application.Auth.PasskeySignIn;

internal sealed class PasskeySignInHandler(ISignInManager signInManager) : ICommandHandler<PasskeySignInCommand>
{
    public Task<Result> Handle(PasskeySignInCommand command, CancellationToken cancellationToken = default) =>
        signInManager.PasskeySignInAsync(
            command.Credential,
            command.State,
            command.UseCookies,
            command.UseSessionCookies);
}
