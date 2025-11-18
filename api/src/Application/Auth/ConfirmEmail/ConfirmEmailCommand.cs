using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Auth.ConfirmEmail;

public sealed record ConfirmEmailCommand(string Email, string Code, string? ChangedEmail) : ICommand;