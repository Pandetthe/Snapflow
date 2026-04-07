using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Boards.GetDetails;

internal sealed class GetBoardDetailsHandler(
    IAppDbContext context,
    IAvatarService avatarService) : IQueryHandler<GetBoardDetailsQuery, GetBoardDetailsResponse>
{
    public async Task<Result<GetBoardDetailsResponse>> Handle(GetBoardDetailsQuery query, CancellationToken cancellationToken = default)
    {
        var dbBoard = await context.Boards
            .AsNoTracking()
            .Where(b => b.Id == query.Id && !b.IsDeleted)
            .Select(b => new
            {
                b.Id,
                b.Title,
                b.Description,
                Members = b.Members.Select(m => new
                {
                    m.UserId,
                    m.User.UserName,
                    m.User.Email,
                    m.User.AvatarType,
                    m.Role
                }).ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (dbBoard == null)
            return Result.Failure<GetBoardDetailsResponse>(BoardErrors.NotFound(query.Id));

        var members = dbBoard.Members.Select(m =>
        {
            var avatarUrl = m.AvatarType switch
            {
                AvatarType.Gravatar => avatarService.GetGravatarUrl(m.Email),
                _ => avatarService.GenerateAvatarUrl(m.UserId)
            };

            return new GetBoardDetailsMemberResponse(m.UserId, m.UserName, avatarUrl, m.Role);
        }).ToList();

        var response = new GetBoardDetailsResponse(
            dbBoard.Id,
            dbBoard.Title,
            dbBoard.Description,
            members);

        return response;
    }
}
