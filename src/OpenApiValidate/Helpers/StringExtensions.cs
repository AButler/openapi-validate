namespace OpenApiValidate;

internal static class StringExtensions
{
    extension(string input)
    {
        public string EnsureEndsWith(string end)
        {
            if (input.EndsWith(end, StringComparison.Ordinal))
            {
                return input;
            }

            return input + end;
        }

        public string EnsureStartsWith(string start)
        {
            if (input.StartsWith(start, StringComparison.Ordinal))
            {
                return input;
            }

            return start + input;
        }
    }
}
