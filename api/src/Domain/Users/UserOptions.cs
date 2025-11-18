namespace Snapflow.Domain.Users;

public sealed class UserOptions
{
    public const int MinPasswordLength = 8;
    public const int MaxPasswordLength = 64;
    public const int MaxEmailLength = 254;
    public const bool RequireLowercaseInPassword = true;
    public const bool RequireUppercaseInPassword = true;
    public const bool RequireDigitInPassword = true;
    public const bool RequireNonAlphanumericInPassword = true;
    public const int MaxUserNameLength = 20;
    public const int MinUserNameLength = 3;
}
