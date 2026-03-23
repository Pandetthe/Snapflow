using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me;

public sealed record MeResponse(int Id, string UserName, string Email, string? AvatarUrl, AvatarType AvatarType);
