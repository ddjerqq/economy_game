using System.Security.Cryptography;
using System.Text;

namespace Domain.Common;

public static class StringExt
{
    public static (string FirstName, string LastName) SplitName(this string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        var names = name.Split(' ');
        return names switch
        {
            [var first, var last] => (first, last),
            [var first, var last, var rest] => (first, $"{last} {rest}"),
            _ => throw new ArgumentException($"Invalid name: {name}", nameof(name)),
        };
    }

    public static string Abbreviate(this string name)
    {
        var (first, last) = name.SplitName();
        return $"{first.FirstOrDefault()}. {last}";
    }

    public static string ToSnakeCase(this string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        if (text.Length < 2)
            return text;

        var sb = new StringBuilder();
        sb.Append(char.ToLowerInvariant(text[0]));

        for (var i = 1; i < text.Length; ++i)
        {
            var c = text[i];
            if (char.IsUpper(c))
            {
                sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    public static string Sha256Hash(this string value) => Convert.ToHexString(
        SHA256.HashData(Encoding.UTF8.GetBytes(value)))
        .ToLower();

    public static string? FromEnv(this string key) =>
        Environment.GetEnvironmentVariable(key);

    public static string FromEnv(this string key, string value) =>
        Environment.GetEnvironmentVariable(key) ?? value;

    public static string FromEnvRequired(this string key) =>
        Environment.GetEnvironmentVariable(key)
        ?? throw new InvalidOperationException($"Environment variable not found: {key}");
}