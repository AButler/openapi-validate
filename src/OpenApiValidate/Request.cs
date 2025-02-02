namespace OpenApiValidate;

public record Request(string Method, Uri Uri, string? ContentType = null, string? Body = null);
