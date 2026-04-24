using System.ComponentModel.DataAnnotations;

namespace Snapflow.Infrastructure.Avatars;

public sealed class AvatarOptions
{
    [Range(1, 50)]
    public int MaxFileSize { get; set; } = 5;

    [MinLength(1)]
    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".gif", ".webp"];
}
