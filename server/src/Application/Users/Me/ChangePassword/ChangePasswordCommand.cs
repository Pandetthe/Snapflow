using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Users.Me.ChangePassword;

public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword) : ICommand;
