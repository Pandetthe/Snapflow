using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;

namespace Snapflow.Application.Auth.SignOut;

internal sealed class SignOutCommandHandler(
    ISignInManager signInManager) : ICommandHandler<SignOutCommand>
{
    public async Task<Result> Handle(SignOutCommand command, CancellationToken cancellationToken = default)
    {
        await signInManager.SignOutAsync();
        return Result.Success();
    }
}