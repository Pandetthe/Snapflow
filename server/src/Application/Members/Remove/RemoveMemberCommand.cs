using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Members.Remove;

public sealed record RemoveMemberCommand(int BoardId, int UserId) : ICommand;