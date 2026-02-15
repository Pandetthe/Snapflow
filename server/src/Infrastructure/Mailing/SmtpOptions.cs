using MailKit.Security;

namespace Snapflow.Infrastructure.Mailing;

public sealed class SmtpOptions
{
    public string Host { get; init; } = "";
    public int Port { get; init; } = 25;
    public SecureSocketOptions SecureSocketOptions { get; init; }
    public bool RequireAuthentication { get; init; }
    public string UserName { get; init; } = "";
    public string Password { get; init; } = "";
    public string FromName { get; init; } = "No Reply";
    public string FromEmail { get; init; } = "noreply@snapflow.pl";
}