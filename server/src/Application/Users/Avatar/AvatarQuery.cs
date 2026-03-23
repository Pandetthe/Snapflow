using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Users.Avatar;

public sealed record AvatarQuery(int UserId) : IQuery<AvatarResponse>;