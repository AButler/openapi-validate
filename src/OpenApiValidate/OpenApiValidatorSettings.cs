namespace OpenApiValidate;

public record OpenApiValidatorSettings
{
    public IDictionary<string, string> ServerAliases { get; } = new Dictionary<string, string>();
    public bool ValidateRequest { get; set; } = true;
    public bool ValidateResponse { get; set; } = true;
    public bool ValidateResponseContentTypeIfNotSuccess { get; set; } = true;
}
