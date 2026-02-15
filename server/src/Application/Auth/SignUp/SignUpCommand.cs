using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Auth.SignUp;

public sealed record SignUpCommand(string UserName, string Email, string Password) : ICommand;