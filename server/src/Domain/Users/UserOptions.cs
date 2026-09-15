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
    public const int TwoFactorRecoveryCodeCount = 10;
    public const int MaxTwoFactorCodeLength = 32;
    public const int MaxTwoFactorTokenLength = 4096;
    public const int MaxPasskeysPerUser = 20;
    public const int MaxPasskeyNameLength = 50;
    public const int MaxPasskeyIdLength = 1400;
    public const int MaxPasskeyCredentialLength = 16384;
    public const int MaxPasskeyStateLength = 8192;
}
