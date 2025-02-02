namespace OpenApiValidate;

public record Response(int StatusCode, string? ContentType = null, string? Body = null);
