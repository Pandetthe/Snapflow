using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.UpdateAvatar;

public sealed record UpdateAvatarCommand(
    AvatarType AvatarType,
    byte[]? AvatarData,
    string? ContentType) : ICommand;
