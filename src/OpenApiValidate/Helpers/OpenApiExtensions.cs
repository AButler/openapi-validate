using Json.Schema;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Models.Interfaces;
using Microsoft.OpenApi.Writers;

namespace OpenApiValidate;

internal static class OpenApiExtensions
{
    public static JsonSchema ToJsonSchema(this IOpenApiSchema schema)
    {
        var writer = new StringWriter();
        var writerSettings = new OpenApiWriterSettings
        {
            InlineExternalReferences = true,
            InlineLocalReferences = true,
        };
        schema.SerializeAsV31(new OpenApiJsonWriter(writer, writerSettings));

        var json = writer.ToString();
        return JsonSchema.FromText(json);
    }

    public static bool TryMatchResponse(
        this OpenApiResponses responses,
        int statusCode,
        out IOpenApiResponse response
    )
    {
        if (responses.TryGetValue(statusCode.ToString(), out var statusCodeMatchResponse))
        {
            response = statusCodeMatchResponse;
            return true;
        }

        var range = statusCode switch
        {
            >= 100 and <= 199 => "1XX",
            >= 200 and <= 299 => "2XX",
            >= 300 and <= 399 => "3XX",
            >= 400 and <= 499 => "4XX",
            >= 500 and <= 599 => "5XX",
            _ => null,
        };

        if (range != null && responses.TryGetValue(range, out var rangeMatchResponse))
        {
            response = rangeMatchResponse;
            return true;
        }

        response = null!;
        return false;
    }

    public static bool TryMatchPath(
        this OpenApiPaths paths,
        string requestPath,
        out IOpenApiPathItem path
    )
    {
        var requestPathString = new PathString(requestPath);

        foreach (var kvp in paths)
        {
            var specPath = new PathString(kvp.Key);
            if (IsPathMatch(specPath, requestPathString))
            {
                path = kvp.Value;
                return true;
            }
        }

        path = null!;
        return false;
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
}
