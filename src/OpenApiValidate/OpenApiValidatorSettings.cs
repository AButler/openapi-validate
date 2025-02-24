namespace OpenApiValidate;

public class OpenApiValidatorSettings
{
    public IDictionary<string, string> ServerAliases { get; } = new Dictionary<string, string>();
    public bool ValidateRequest { get; set; } = true;
    public bool ValidateResponse { get; set; } = true;

    public StatusCodeList ValidateResponseContentTypeIfStatusCode { get; set; } =
        StatusCodeList.All;
}
