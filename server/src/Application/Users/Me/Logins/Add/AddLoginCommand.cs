using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Users.Me.Logins.Add;

public sealed record AddLoginCommand : ICommand<string>;
