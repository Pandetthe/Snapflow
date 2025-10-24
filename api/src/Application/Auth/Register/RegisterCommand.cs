using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Auth.Register;

public sealed record RegisterCommand(string UserName, string Email, string Password) : ICommand;