using System.Text;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Avatar;

internal sealed class AvatarHandler(
    IUserManager userManager,
    IAvatarService avatarService) : IQueryHandler<AvatarQuery, AvatarResponse>
{
    public async Task<Result<AvatarResponse>> Handle(AvatarQuery query, CancellationToken cancellationToken)
    {
        IUser? user = await userManager.FindByIdAsync(query.UserId);
        
        if (user is null)
            return Result.Failure<AvatarResponse>(UserErrors.NotFound(query.UserId));
        
        return user.AvatarType switch
        {
            AvatarType.Uploaded => user.AvatarData is { Length: > 0 } 
                ? Result.Success(new AvatarResponse(user.AvatarData, "image/png")) 
                : Result.Failure<AvatarResponse>(UserErrors.AvatarDataMissing),

            AvatarType.Generated => Result.Success(new AvatarResponse(
                Encoding.UTF8.GetBytes(avatarService.GenerateJdenticonSvg(user.Id)), 
                "image/svg+xml")),

            _ => Result.Failure<AvatarResponse>(UserErrors.AvatarInvalidFileType)
        };
    }
}