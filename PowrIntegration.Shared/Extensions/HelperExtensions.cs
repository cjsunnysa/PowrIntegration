using System.Text.RegularExpressions;

namespace PowrIntegration.Shared.Extensions;

public static class HelperExtensions
{
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Replace capital letters with "_lowercase"
        string snakeCase = Regex.Replace(input, @"([a-z0-9])([A-Z])", "$1_$2");

        // Replace spaces or non-alphanumeric characters with underscores
        snakeCase = Regex.Replace(snakeCase, @"\s+", "_");

        // Ensure it's all lowercase
        return snakeCase.ToLowerInvariant();
    }
}
