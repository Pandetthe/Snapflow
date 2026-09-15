using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Users.Me.TwoFactor.Enable;

public sealed record EnableTwoFactorCommand(string Code) : ICommand<EnableTwoFactorResponse>;
