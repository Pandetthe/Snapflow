using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Users.Me.SetPassword;

public sealed record SetPasswordCommand(string NewPassword) : ICommand;
