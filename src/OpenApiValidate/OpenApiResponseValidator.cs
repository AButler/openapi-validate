using System.Text.Json.Nodes;
using Microsoft.OpenApi.Models;

namespace OpenApiValidate;

public class OpenApiResponseValidator(OpenApiDocument openApiDocument)
{
    public void Validate(Request request, Response response)
    {
        if (!TryValidate(request, response))
        {
            throw new Exception("The validation failed.");
        }
    }

    public bool TryValidate(Request request, Response response)
    {
        var server = openApiDocument.Servers.FirstOrDefault(s =>
            request.Uri.ToString().StartsWith(s.Url)
        );

        if (server == null)
        {
            //TODO: add error
            return false;
        }

        var requestPath = MakeRelativePath(server, request.Uri);

        if (!openApiDocument.Paths.TryGetValue(requestPath, out var path))
        {
            //TODO: add error
            return false;
        }

        var operationType = ToOperationType(request.Method);

        if (!path.Operations.TryGetValue(operationType, out var operation))
        {
            //TODO: add error
            return false;
        }

        //TODO: status code ranges, e.g. 5XX
        if (!operation.Responses.TryGetValue(response.StatusCode.ToString(), out var responseModel))
        {
            //TODO: add error
            return false;
        }

        if (!responseModel.Content.TryGetValue(response.ContentType, out var contentType))
        {
            //TODO: add error
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
            //TODO: add error
            return false;
        }

        return true;
    }

    private static string MakeRelativePath(OpenApiServer server, Uri requestUri)
    {
        var serverUrl = server.Url.EndsWith("/") ? server.Url : server.Url + "/";
        var serverUri = new Uri(serverUrl);
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

public record Request(string Method, Uri Uri);

public record Response(int StatusCode, string? ContentType, string? Body = null);
