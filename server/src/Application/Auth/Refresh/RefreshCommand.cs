using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Auth.Refresh;

public sealed record RefreshCommand(string RefreshToken) : ICommand;