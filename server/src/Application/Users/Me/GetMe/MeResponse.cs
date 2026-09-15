using Snapflow.Domain.Users;

namespace Snapflow.Application.Users.Me.GetMe;

public sealed record MeResponse(int Id, string UserName, string Email, bool EmailConfirmed, string? AvatarUrl, AvatarType AvatarType);
