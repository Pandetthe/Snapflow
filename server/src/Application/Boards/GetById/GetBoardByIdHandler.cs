using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Boards;
using static Snapflow.Application.Boards.GetById.GetBoardByIdResponse;

namespace Snapflow.Application.Boards.GetById;

internal sealed class GetBoardByIdHandler(
    IAppDbContext context,
    IAvatarService avatarService,
    IBoardMembershipService membershipService) : IQueryHandler<GetBoardByIdQuery, GetBoardByIdResponse>
{
    public async Task<Result<GetBoardByIdResponse>> Handle(GetBoardByIdQuery query, CancellationToken cancellationToken = default)
    {
        bool isMember = await membershipService.IsMemberAsync(query.Id, cancellationToken);

        GetBoardByIdResponse? board = await context.Boards
            .AsNoTracking()
            .Where(b => b.Id == query.Id)
            .Select(b => new GetBoardByIdResponse(
                b.Id,
                b.Title,
                b.Description,
                b.Visibility,
                b.Swimlanes
                    .OrderBy(s => s.Rank)
                    .Select(s => new SwimlaneDto(
                        s.Id,
                        s.Title,
                        s.Rank,
                        s.Height,
                        s.Lists
                            .OrderBy(l => l.Rank)
                            .Select(l => new ListDto(
                                l.Id,
                                l.Title,
                                l.Rank,
                                l.Width,
                                l.Cards
                                    .OrderBy(c => c.Rank)
                                    .Select(c => new CardDto(
                                        c.Id,
                                        c.Title,
                                        c.Description,
                                        c.Rank,
                                        c.CreatedAt,
                                        isMember ? UserDto.From(c.CreatedBy) : null,
                                        c.UpdatedAt,
                                        isMember ? UserDto.From(c.UpdatedBy) : null,
                                        c.Tags
                                            .Select(t => t.Id)
                                            .ToList()))
                                    .ToList()))
                            .ToList()))
                    .ToList(),
                b.Tags
                    .OrderBy(t => t.Title)
                    .Select(t => new TagDto(t.Id, t.Title, t.Color))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);

        if (board == null)
            return BoardErrors.NotFound(query.Id);

        return board with
        {
            Swimlanes = [.. board.Swimlanes.Select(s => s with
            {
                Lists = [.. s.Lists.Select(l => l with
                {
                    Cards = [.. l.Cards.Select(c => c with
                    {
                        CreatedBy = WithAvatar(c.CreatedBy),
                        UpdatedBy = WithAvatar(c.UpdatedBy)
                    })]
                })]
            })]
        };
    }

    private UserDto? WithAvatar(UserDto? user) =>
        user is null ? null : user with { AvatarUrl = avatarService.GenerateAvatarUrl(user.Id) };
}
