using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Members.ChangeOwner;

public sealed record ChangeOwnerCommand(int UserId, int BoardId) : ICommand;