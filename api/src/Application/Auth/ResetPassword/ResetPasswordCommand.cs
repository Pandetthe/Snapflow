using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Auth.ResetPassword;

public sealed record ResetPasswordCommand(string Email, string ResetCode, string NewPassword) : ICommand;