using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Users.Me.Logins.Remove;

public sealed record RemoveLoginCommand(string Provider) : ICommand;
