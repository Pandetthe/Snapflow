using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Users.Me.UpdateProfile;

public sealed record UpdateProfileCommand(string UserName) : ICommand;
