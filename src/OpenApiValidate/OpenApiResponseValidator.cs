using System.Text.Json.Nodes;
using Microsoft.OpenApi.Models;

namespace OpenApiValidate;

public class OpenApiResponseValidator(OpenApiDocument openApiDocument)
{
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

        //TODO: No servers?
        var server = openApiDocument.Servers.FirstOrDefault(s =>
            request.Uri.ToString().StartsWith(s.Url)
        );

        if (server == null)
        {
            validationErrors.Add(new ValidationError("Server not found"));
            return false;
        }

        var requestPath = MakeRelativePath(server, request.Uri);

        var path = FindPath(openApiDocument, requestPath);

        if (path == null)
        {
            validationErrors.Add(new ValidationError("Path not found"));
            return false;
        }

        var operationType = ToOperationType(request.Method);

        if (!path.Operations.TryGetValue(operationType, out var operation))
        {
            validationErrors.Add(new ValidationError($"Operation not found: {request.Method}"));
            return false;
        }

        //TODO: status code ranges, e.g. 5XX
        if (!operation.Responses.TryGetValue(response.StatusCode.ToString(), out var responseModel))
        {
            validationErrors.Add(new ValidationError($"Response not found: {response.StatusCode}"));
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
                new ValidationError($"Content type not found: {response.ContentType}")
            );
            return false;
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

    private static OpenApiPathItem? FindPath(OpenApiDocument document, PathString requestPath)
    {
        foreach (var kvp in document.Paths)
        {
            var specPath = new PathString(kvp.Key);
            if (IsPathMatch(specPath, requestPath))
            {
                return kvp.Value;
            }
        }

        return null;
    }

    private static bool IsPathMatch(PathString specPath, PathString requestPath)
    {
        if (specPath.Segments.Length != requestPath.Segments.Length)
        {
            return false;
        }

        for (var i = 0; i < specPath.Segments.Length; i++)
        {
            var segment = specPath.Segments[i];

            if (segment.StartsWith("{") && segment.EndsWith("}"))
            {
                // Is template parameter, so skip checking
                continue;
            }

            if (
                !segment.Equals(
                    requestPath.Segments[i],
                    StringComparison.InvariantCultureIgnoreCase
                )
            )
            {
                return false;
            }
        }

        return true;
    }

    private static PathString MakeRelativePath(OpenApiServer server, Uri requestUri)
    {
        var serverUrl = server.Url.EndsWith("/") ? server.Url : server.Url + "/";
        var serverUri = new Uri(serverUrl);
        var relativeUri = serverUri.MakeRelativeUri(requestUri);

        return new PathString("/" + relativeUri);
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

public record ValidationError(string Message);

public record Request(string Method, Uri Uri);

public record Response(int StatusCode, string? ContentType = null, string? Body = null);
