using System.Text;
using System.Text.Json.Nodes;
using Json.Schema;
using Microsoft.OpenApi;

namespace OpenApiValidate;

public class OpenApiValidator
{
    private static readonly EvaluationOptions JsonSchemaEvaluationOptions = new()
    {
        OutputFormat = OutputFormat.List,
    };

    private readonly OpenApiDocument _openApiDocument;
    private readonly OpenApiValidatorSettings _settings;

    public OpenApiValidator(
        OpenApiDocument openApiDocument,
        OpenApiValidatorSettings? settings = null
    )
    {
        ArgumentNullException.ThrowIfNull(openApiDocument, nameof(openApiDocument));

        _openApiDocument = openApiDocument;
        _settings = settings ?? new OpenApiValidatorSettings();

        new OpenApiSettingsValidator(_openApiDocument, _settings).Validate();
    }

    public void Validate(Request request, Response response)
    {
        if (!TryValidate(request, response, out var validationErrors))
        {
            throw new OpenApiValidationException(validationErrors);
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
                    new ValidationError(
                        $"No server found that matched the request: '{request.Uri}'"
                    )
                );
                return false;
            }

            var serverUrl = _settings.ServerAliases.GetValueOrDefault(server.Url!, server.Url!);
            requestPath = MakeRelativePath(serverUrl, request.Uri);
        }

        if (!_openApiDocument.Paths.TryMatchPath(requestPath, out var path))
        {
            validationErrors.Add(
                new ValidationError($"No path found that matched the request path: '{requestPath}'")
            );
            return false;
        }

        var operationType = ToHttpMethod(request.Method);

        if (
            path.Operations == null
            || !path.Operations.TryGetValue(operationType, out var operation)
        )
        {
            validationErrors.Add(
                new ValidationError(
                    $"No operation found that matched the request method: '{request.Method}'"
                )
            );
            return false;
        }

        if (
            _settings.ValidateRequest
            && !TryValidateRequest(request, operation, out var requestValidationErrors)
        )
        {
            validationErrors.AddRange(requestValidationErrors);
            return false;
        }

        if (
            _settings.ValidateResponse
            && !TryValidateResponse(response, operation, out var responseValidationErrors)
        )
        {
            validationErrors.AddRange(responseValidationErrors);
            return false;
        }

        return true;
    }

    private bool TryValidateRequest(
        Request request,
        OpenApiOperation operation,
        out List<ValidationError> validationErrors
    )
    {
        validationErrors = [];

        if (operation.RequestBody == null)
        {
            // Nothing to validate
            return true;
        }

        if (request.Body == null)
        {
            if (!operation.RequestBody.Required)
            {
                // Body not required
                return true;
            }

            validationErrors.Add(new ValidationError("Request body is required"));
            return false;
        }

        if (request.ContentType == null)
        {
            // No content type, therefore no body
            //TODO: check if has body and no content type
            return true;
        }

        if (
            operation.RequestBody.Content == null
            || !operation.RequestBody.Content.TryGetValue(request.ContentType, out var contentType)
        )
        {
            validationErrors.Add(
                new ValidationError(
                    $"No content type found that matched the request content type: '{request.ContentType}'"
                )
            );
            return false;
        }

        if (contentType.Schema == null)
        {
            //No schema - does this mean no body?
            return false;
        }

        if (!TryValidateSchema(contentType.Schema, "Request", request.Body, out var validateErrors))
        {
            validationErrors.AddRange(validateErrors);
            return false;
        }

        return true;
    }

    private bool TryValidateResponse(
        Response response,
        OpenApiOperation operation,
        out List<ValidationError> validationErrors
    )
    {
        validationErrors = [];

        if (
            operation.Responses == null
            || !operation.Responses.TryMatchResponse(response.StatusCode, out var responseModel)
        )
        {
            validationErrors.Add(
                new ValidationError(
                    $"No status code found that matched the response status code: {response.StatusCode}"
                )
            );
            return false;
        }

        if (
            !_settings.ValidateResponseContentTypeIfStatusCode.ContainsStatusCode(
                response.StatusCode
            )
        )
        {
            return true;
        }

        if (response.ContentType == null)
        {
            // No content type, therefore no body
            //TODO: check if has body and no content type
            return true;
        }

        if (
            responseModel.Content == null
            || !responseModel.Content.TryGetValue(response.ContentType, out var contentType)
        )
        {
            validationErrors.Add(
                new ValidationError(
                    $"No content type found that matched the response content type: '{response.ContentType}'"
                )
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

        if (
            !TryValidateSchema(
                contentType.Schema,
                "Response",
                response.Body,
                out var validateErrors
            )
        )
        {
            validationErrors.AddRange(validateErrors);
            return false;
        }

        return true;
    }

    private static bool TryValidateSchema(
        IOpenApiSchema schema,
        string bodyType,
        string body,
        out List<ValidationError> validationErrors
    )
    {
        validationErrors = [];

        var jsonSchema = schema.ToJsonSchema();
        var validationResult = jsonSchema.Evaluate(
            JsonNode.Parse(body),
            JsonSchemaEvaluationOptions
        );

        if (!validationResult.IsValid)
        {
            var message = new StringBuilder();

            foreach (var detail in validationResult.Details.Where(d => d.HasErrors))
            {
                var path = detail.EvaluationPath.ToString();

                foreach (var error in detail.Errors!)
                {
                    message.AppendLine($"[{error.Key}] {path}: {error.Value}");
                }
            }

            validationErrors.Add(
                new ValidationError($"{bodyType} body failed schema validation: \n\n" + message)
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
            if (server.Url == null)
            {
                continue;
            }

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

    private static HttpMethod ToHttpMethod(string method)
    {
        return method.ToUpperInvariant() switch
        {
            "GET" => HttpMethod.Get,
            "POST" => HttpMethod.Post,
            "PUT" => HttpMethod.Put,
            "PATCH" => HttpMethod.Patch,
            "DELETE" => HttpMethod.Delete,
            "HEAD" => HttpMethod.Head,
            "OPTIONS" => HttpMethod.Options,
            "TRACE" => HttpMethod.Trace,
            _ => throw new ArgumentException($"Unknown operation type: {method}"),
        };
    }
}
