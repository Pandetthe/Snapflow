using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.GetPasswordStatus;

internal sealed class GetPasswordStatusHandler(
    IUserContext userContext,
    IUserManager userManager) : IQueryHandler<GetPasswordStatusQuery, GetPasswordStatusResponse>
{
    public async Task<Result<GetPasswordStatusResponse>> Handle(GetPasswordStatusQuery query, CancellationToken cancellationToken = default)
    {
        IUser user = await userContext.GetUserAsync();
        return new GetPasswordStatusResponse(await userManager.HasPasswordAsync(user));
    }
}
