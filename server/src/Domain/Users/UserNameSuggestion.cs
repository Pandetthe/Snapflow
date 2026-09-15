using System.Globalization;
using System.Text;

namespace Snapflow.Domain.Users;

public static class UserNameSuggestion
{
    private const string Fallback = "user";

    public static string From(string? name, string? email)
    {
        string candidate = Sanitize(name);
        if (candidate.Length < UserOptions.MinUserNameLength)
            candidate = Sanitize(email?.Split('@')[0]);
        if (candidate.Length < UserOptions.MinUserNameLength)
            candidate = Fallback;

        return Truncate(candidate, UserOptions.MaxUserNameLength);
    }

    public static string WithNumber(string suggestion, int number)
    {
        string suffix = number.ToString(CultureInfo.InvariantCulture);
        return Truncate(suggestion, UserOptions.MaxUserNameLength - suffix.Length).TrimEnd('.', '-', '_') + suffix;
    }

    private static string Truncate(string value, int length) =>
        value.Length > length ? value[..length].TrimEnd('.', '-', '_') : value;

    private static string Sanitize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var builder = new StringBuilder(value.Length);
        bool lastWasSeparator = true;

        foreach (char c in value.Trim().Normalize(NormalizationForm.FormD))
        {
            char mapped = c switch
            {
                'ł' => 'l',
                'Ł' => 'L',
                _ => c
            };

            if (char.IsAsciiLetterOrDigit(mapped))
            {
                builder.Append(mapped);
                lastWasSeparator = false;
            }
            else if ((char.IsWhiteSpace(mapped) || mapped is '.' or '-' or '_') && !lastWasSeparator)
            {
                builder.Append(char.IsWhiteSpace(mapped) ? '.' : mapped);
                lastWasSeparator = true;
            }
        }

        return builder.ToString().TrimEnd('.', '-', '_');
    }
}
