namespace OpenApiValidate;

public class OpenApiValidationException : Exception
{
    public OpenApiValidationException(IEnumerable<ValidationError> errors)
        : base(BuildMessage(errors.ToList())) { }

    private static string BuildMessage(List<ValidationError> validationErrors)
    {
        return validationErrors.Count == 0
            ? "Validation failed"
            : string.Join("\n", validationErrors.Select(e => e.Message));
    }
}
