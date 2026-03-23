using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Auth.Refresh;

internal sealed class RefreshCommandHandler(
    ISignInManager signInManager) : ICommandHandler<RefreshCommand>
{
    public async Task<Result> Handle(RefreshCommand command, CancellationToken cancellationToken = default)
    {
        return await signInManager.RefreshSignInAsync(command.RefreshToken);
    }
}