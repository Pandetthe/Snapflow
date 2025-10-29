using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.BoardMembers.Get;

public sealed record GetBoardMembersQuery : IQuery<GetBoardMembersResponse>;