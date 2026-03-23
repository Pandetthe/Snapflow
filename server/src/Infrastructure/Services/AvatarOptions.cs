namespace Snapflow.Infrastructure.Services;

public sealed class AvatarOptions
{
    public int MaxFileSize { get; set; } = 5;

    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
}