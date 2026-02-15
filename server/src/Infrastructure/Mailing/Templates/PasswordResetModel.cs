namespace Snapflow.Infrastructure.Mailing.Templates;

public sealed class PasswordResetModel
{
    public required string UserName { get; init; }
    public required string ResetFormLink { get; init; }

    public const string TemplateName = "PasswordReset";
}
