using System.Text;

namespace Snapflow.Infrastructure.Auth.External;

public static class LdapFilter
{
    public static string Escape(string value)
    {
        var builder = new StringBuilder(value.Length);

        foreach (char c in value)
        {
            builder.Append(c switch
            {
                '\\' => @"\5c",
                '*' => @"\2a",
                '(' => @"\28",
                ')' => @"\29",
                '\0' => @"\00",
                _ => c.ToString()
            });
        }

        return builder.ToString();
    }
}
