using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.TwoFactor.GetStatus;

internal sealed class GetTwoFactorStatusHandler(
    IUserContext userContext,
    IUserManager userManager) : IQueryHandler<GetTwoFactorStatusQuery, GetTwoFactorStatusResponse>
{
    public async Task<Result<GetTwoFactorStatusResponse>> Handle(GetTwoFactorStatusQuery query, CancellationToken cancellationToken = default)
    {
        IUser user = await userContext.GetUserAsync();
        TwoFactorStatus status = await userManager.GetTwoFactorStatusAsync(user);

        return new GetTwoFactorStatusResponse(status.IsEnabled, status.RecoveryCodesLeft);
    }
}
