using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;

namespace Snapflow.Application.Users.Me;

internal sealed class MeQueryHandler(IUserContext userContext) : IQueryHandler<MeQuery, MeResponse>
{
    public async Task<Result<MeResponse>> Handle(MeQuery query, CancellationToken cancellationToken = default)
    {
        var user = await userContext.GetUserAsync();
        return new MeResponse(user.Id, user.UserName, user.Email);
    }
}