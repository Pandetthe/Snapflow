using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Auth.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : ICommand;