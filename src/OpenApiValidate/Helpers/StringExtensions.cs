namespace OpenApiValidate;

internal static class StringExtensions
{
    public static string EnsureEndsWith(this string input, string end)
    {
        if (input.EndsWith(end, StringComparison.Ordinal))
        {
            return input;
        }

        return input + end;
    }

    public static string EnsureStartsWith(this string input, string start)
    {
        if (input.StartsWith(start, StringComparison.Ordinal))
        {
            return input;
        }

        return start + input;
    }
}
