using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Auth.ConfirmEmail;

public sealed record ConfirmEmailCommand(int UserId, string Code, string? ChangedEmail) : ICommand;