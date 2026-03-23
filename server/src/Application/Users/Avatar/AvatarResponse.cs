namespace Snapflow.Application.Users.Avatar;

public sealed record AvatarResponse(
    byte[] Data, 
    string ContentType);