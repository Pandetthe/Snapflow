using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Boards;

namespace Snapflow.Application.Boards.GetDetails;

internal sealed class GetBoardDetailsHandler(
    IAppDbContext context,
    IAvatarService avatarService,
    IBoardMembershipService membershipService) : IQueryHandler<GetBoardDetailsQuery, GetBoardDetailsResponse>
{
    public async Task<Result<GetBoardDetailsResponse>> Handle(GetBoardDetailsQuery query, CancellationToken cancellationToken = default)
    {
        bool isMember = await membershipService.IsMemberAsync(query.Id, cancellationToken);

        var dbBoard = await context.Boards
            .AsNoTracking()
            .Where(b => b.Id == query.Id && !b.IsDeleted)
            .Select(b => new
            {
                b.Id,
                b.Title,
                b.Description,
                b.Visibility,
                Members = b.Members.Select(m => new
                {
                    m.UserId,
                    m.User.UserName,
                    m.Role
                }).ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (dbBoard == null)
            return Result.Failure<GetBoardDetailsResponse>(BoardErrors.NotFound(query.Id));

        var members = dbBoard.Members
            .Where(_ => isMember)
            .Select(m => new GetBoardDetailsMemberResponse(
                m.UserId, m.UserName, avatarService.GenerateAvatarUrl(m.UserId), m.Role))
            .ToList();

        var response = new GetBoardDetailsResponse(
            dbBoard.Id,
            dbBoard.Title,
            dbBoard.Description,
            dbBoard.Visibility,
            members);

        return response;
    }
}
