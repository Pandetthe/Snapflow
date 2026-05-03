using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Users.Me.RequestEmailChange;

public sealed record RequestEmailChangeCommand(string NewEmail) : ICommand;
