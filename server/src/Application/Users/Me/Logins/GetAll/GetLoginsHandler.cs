using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.Logins.GetAll;

internal sealed class GetLoginsHandler(
    IUserContext userContext,
    IUserManager userManager) : IQueryHandler<GetLoginsQuery, IReadOnlyList<ExternalLoginDetails>>
{
    public async Task<Result<IReadOnlyList<ExternalLoginDetails>>> Handle(GetLoginsQuery query, CancellationToken cancellationToken = default)
    {
        IUser user = await userContext.GetUserAsync();
        return Result.Success(await userManager.GetLoginsAsync(user));
    }
}
