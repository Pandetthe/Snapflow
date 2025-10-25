using Snapflow.Domain.Users;

namespace Snapflow.Application.Abstractions.Identity;

public interface IUserContext
{
    int UserId { get; }

    string UserName { get; }

    Task<IUser> GetUserAsync();

    bool IsAuthenticated { get; }
}
