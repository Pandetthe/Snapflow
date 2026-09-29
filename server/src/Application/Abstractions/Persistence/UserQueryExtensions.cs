using Microsoft.EntityFrameworkCore;

namespace Snapflow.Application.Abstractions.Persistence;

public static class UserQueryExtensions
{
    extension(IAppDbContext dbContext)
    {
        public Task<string?> FindUserNameAsync(int userId, CancellationToken cancellationToken) =>
            dbContext.Users
                .Where(u => u.Id == userId)
                .Select(u => (string?)u.UserName)
                .SingleOrDefaultAsync(cancellationToken);
    }
}
