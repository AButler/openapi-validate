using System.Text.Json.Nodes;
using Microsoft.OpenApi.Models;

namespace OpenApiValidate;

public class OpenApiResponseValidator
{
    private readonly OpenApiDocument _openApiDocument;
    private readonly OpenApiValidatorSettings _settings;

    public OpenApiResponseValidator(
        OpenApiDocument openApiDocument,
        OpenApiValidatorSettings? settings = null
    )
    {
        _openApiDocument = openApiDocument;
        _settings = settings ?? new OpenApiValidatorSettings();

        ValidateSettings();
    }

    private void ValidateSettings()
    {
        if (_settings.ServerAliases.Count == 0)
        {
            return;
        }

        if (_openApiDocument.Servers == null)
        {
            throw new InvalidOperationException(
                $"Server '{_settings.ServerAliases.First().Key}' not found"
            );
        }

        foreach (var serverAlias in _settings.ServerAliases.Keys)
        {
            if (_openApiDocument.Servers.All(s => s.Url != serverAlias))
            {
                throw new InvalidOperationException($"Server '{serverAlias}' not found");
            }
        }
    }

    public void Validate(Request request, Response response)
    {
        if (!TryValidate(request, response, out var validationErrors))
        {
            throw new Exception(
                "The validation failed: \n\n"
                    + string.Join("\n", validationErrors.Select(error => error.Message))
            );
        }
    }

    public bool TryValidate(
        Request request,
        Response response,
        out List<ValidationError> validationErrors
    )
    {
        validationErrors = [];

        var requestPath = "/" + request.Uri.AbsolutePath;

        if (_openApiDocument.Servers != null && _openApiDocument.Servers.Any())
        {
            var server = FindServer(request);

            if (server == null)
            {
                validationErrors.Add(
                    new ValidationError($"Server not found matching request '{request.Uri}'")
                );
                return false;
            }

            var serverUrl = _settings.ServerAliases.GetValueOrDefault(server.Url, server.Url);
            requestPath = MakeRelativePath(serverUrl, request.Uri);
        }

        if (!_openApiDocument.Paths.TryMatchPath(requestPath, out OpenApiPathItem path))
        {
            validationErrors.Add(new ValidationError($"Path not found: '{requestPath}'"));
            return false;
        }

        var operationType = ToOperationType(request.Method);

        if (!path.Operations.TryGetValue(operationType, out var operation))
        {
            validationErrors.Add(new ValidationError($"Operation not found: '{request.Method}'"));
            return false;
        }

        if (!operation.Responses.TryMatchResponse(response.StatusCode, out var responseModel))
        {
            validationErrors.Add(
                new ValidationError($"Response status code not found: {response.StatusCode}")
            );
            return false;
        }

        if (response.ContentType == null)
        {
            // No content type, therefore no body
            //TODO: check if has body and no content type
            return true;
        }

        if (!responseModel.Content.TryGetValue(response.ContentType, out var contentType))
        {
            validationErrors.Add(
                new ValidationError($"Response content type not found: '{response.ContentType}'")
            );
            return false;
        }

        if (response.Body == null)
        {
            // No body, nothing left to validate
            return true;
        }

        if (contentType.Schema == null)
        {
            //No schema - does this mean no body?
            return false;
        }

        var jsonSchema = contentType.Schema.ToJsonSchema();
        var validationResult = jsonSchema.Evaluate(JsonNode.Parse(response.Body));

        if (!validationResult.IsValid)
        {
            validationErrors.Add(
                new ValidationError(
                    "Response body did not match schema: \n\n"
                        + string.Join("\n", validationResult.Errors?.Values ?? [])
                )
            );
            return false;
        }

        return true;
    }

    private OpenApiServer? FindServer(Request request)
    {
        if (_openApiDocument.Servers == null)
        {
            return null;
        }

        foreach (var server in _openApiDocument.Servers)
        {
            var replacedServerUri = _settings.ServerAliases.GetValueOrDefault(
                server.Url,
                server.Url
            );

            if (request.Uri.ToString().StartsWith(replacedServerUri))
            {
                return server;
            }
        }

        return null;
    }

    private static string MakeRelativePath(string serverUrl, Uri requestUri)
    {
        var serverUri = new Uri(serverUrl.EnsureEndsWith("/"));
        var relativeUri = serverUri.MakeRelativeUri(requestUri);

        return "/" + relativeUri;
    }

    private static OperationType ToOperationType(string method)
    {
        return method.ToUpperInvariant() switch
        {
            "GET" => OperationType.Get,
            "POST" => OperationType.Post,
            "PUT" => OperationType.Put,
            "PATCH" => OperationType.Patch,
            "DELETE" => OperationType.Delete,
            "HEAD" => OperationType.Head,
            "OPTIONS" => OperationType.Options,
            "TRACE" => OperationType.Trace,
            _ => throw new ArgumentException($"Unknown operation type: {method}"),
        };
    }
}

public record OpenApiValidatorSettings
{
    public IDictionary<string, string> ServerAliases { get; } = new Dictionary<string, string>();
}

public record ValidationError(string Message);

public record Request(string Method, Uri Uri);

public record Response(int StatusCode, string? ContentType = null, string? Body = null);
