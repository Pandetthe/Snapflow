using Snapflow.Application.Abstractions.Authentication;
using Snapflow.Domain.Users;

namespace Snapflow.Infrastructure.Authentication;

internal sealed class TokenProvider : ITokenProvider
{
    public string Create(User user)
    {
        throw new NotImplementedException();
    }
}
