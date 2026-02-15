using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Auth.ResendConfirmationEmail;

public sealed record ResendConfirmationEmailCommand(string Email) : ICommand;