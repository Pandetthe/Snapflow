using Snapflow.Domain.Users;

namespace Snapflow.Application.Abstractions.Authentication;

public interface ITokenProvider
{
    string Create(User user);
}
