using Snapflow.Common;

namespace Snapflow.Domain.Users;

public interface IUser : IEntity<int>
{
    string? UserName { get; }

    string? Email { get; }
}
