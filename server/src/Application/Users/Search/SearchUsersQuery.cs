using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Users.Search;

public sealed record SearchUsersQuery(string Name, IReadOnlySet<int> ExcludedIds) : IQuery<IReadOnlyList<SearchUsersResponse.UserDto>>;