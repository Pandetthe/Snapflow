using System.ComponentModel.DataAnnotations;
using MailKit.Security;

namespace Snapflow.Infrastructure.Mailing;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    [Required(AllowEmptyStrings = false)]
    public string Host { get; init; } = "";

    [Range(1, 65535, ErrorMessage = "Port must be between 1 and 65535.")]
    public int Port { get; init; } = 25;

    public SecureSocketOptions SecureSocketOptions { get; init; }
    public bool RequireAuthentication { get; init; }

    public string UserName { get; init; } = "";
    public string Password { get; init; } = "";

    [Required(AllowEmptyStrings = false)]
    public string FromName { get; init; } = "No Reply";

    [Required(AllowEmptyStrings = false)]
    [EmailAddress]
    public string FromEmail { get; init; } = "noreply@snapflow.pl";
}