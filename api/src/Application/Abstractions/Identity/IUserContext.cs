using Snapflow.Domain.Users;

namespace Snapflow.Application.Abstractions.Identity;

public interface IUserContext
{
    int UserId { get; }

    IUser User { get; }
}
