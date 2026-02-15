using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Members.Get;

public sealed record GetMembersQuery(int BoardId) : IQuery<List<GetMembersResponse>>;