using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Auth.Refresh;

internal sealed class RefreshCommandHandler(
    ISignInManager signInManager,
    IRefreshTokenValidator refreshTokenValidator) : ICommandHandler<RefreshCommand>
{
    public async Task<Result> Handle(RefreshCommand command, CancellationToken cancellationToken = default)
    {
        var refreshTicket = refreshTokenValidator.UnprotectRefreshToken(command.RefreshToken);

        if (refreshTicket == null || refreshTokenValidator.IsTokenExpired(refreshTicket.ExpiresUtc))
            return Result.Failure(UserErrors.RefreshFailed);

        var user = await refreshTokenValidator.ValidateSecurityStampAsync(refreshTicket.ClaimsPrincipal);
        if (user == null)
            return Result.Failure(UserErrors.RefreshFailed);

        await signInManager.RefreshSignInAsync(user);
        return Result.Success();
    }
}