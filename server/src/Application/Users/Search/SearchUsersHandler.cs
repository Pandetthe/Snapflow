using Microsoft.EntityFrameworkCore;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Abstractions.Persistence;
using Snapflow.Application.Abstractions.Services;
using Snapflow.Common;
using Snapflow.Domain.Users;
using static Snapflow.Application.Users.Search.SearchUsersResponse;

namespace Snapflow.Application.Users.Search;

internal sealed class SearchUsersHandler(
    IAppDbContext dbContext,
    IAvatarService avatarService) : IQueryHandler<SearchUsersQuery, IReadOnlyList<UserDto>>
{
    public async Task<Result<IReadOnlyList<UserDto>>> Handle(
        SearchUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        var searchPattern = $"%{query.Name.Trim()}%";

        var users = await dbContext.Users
            .AsNoTracking()
            .Where(u => EF.Functions.ILike(u.UserName, searchPattern))
            .Where(u => !query.ExcludedIds.Contains(u.Id))
            .OrderBy(u => u.UserName)
            .Take(5)
            .ToListAsync(cancellationToken);

        var response = users
            .Select(u =>
            {
                var avatarUrl = u.AvatarType switch
                {
                    AvatarType.Gravatar => avatarService.GetGravatarUrl(u.Email),
                    _ => avatarService.GenerateAvatarUrl(u.Id)
                };

                return new UserDto(u.Id, u.UserName, avatarUrl);
            })
            .ToList();

        return response;
    }
}
