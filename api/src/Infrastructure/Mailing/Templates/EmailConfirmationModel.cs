namespace Snapflow.Infrastructure.Mailing.Templates;

public sealed class EmailConfirmationModel
{
    public required string UserName { get; init; }
    public required string ConfirmationLink { get; init; }

    public const string TemplateName = "EmailConfirmation";
}