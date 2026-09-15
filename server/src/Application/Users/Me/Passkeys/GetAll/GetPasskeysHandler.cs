using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.Passkeys.GetAll;

internal sealed class GetPasskeysHandler(
    IUserContext userContext,
    IPasskeyManager passkeyManager) : IQueryHandler<GetPasskeysQuery, IReadOnlyList<PasskeyDetails>>
{
    public async Task<Result<IReadOnlyList<PasskeyDetails>>> Handle(GetPasskeysQuery query, CancellationToken cancellationToken = default)
    {
        IUser user = await userContext.GetUserAsync();
        return Result.Success(await passkeyManager.GetPasskeysAsync(user));
    }
}
