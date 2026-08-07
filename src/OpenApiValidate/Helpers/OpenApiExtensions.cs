using Json.Schema;
using Microsoft.OpenApi;

namespace OpenApiValidate;

internal static class OpenApiExtensions
{
    private static readonly OpenApiWriterSettings OpenApiWriterSettings = new()
    {
        InlineExternalReferences = true,
        InlineLocalReferences = true,
    };

    private static readonly BuildOptions JsonSchemaBuildOptions = new()
    {
        Dialect = Json.Schema.OpenApi.Dialect.OpenApi_31,
    };

    public static JsonSchema ToJsonSchema(this IOpenApiSchema schema)
    {
        var writer = new StringWriter();

        schema.SerializeAsV31(new OpenApiJsonWriter(writer, OpenApiWriterSettings));

        var json = writer.ToString();

        return JsonSchema.FromText(json, JsonSchemaBuildOptions);
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

        TemplateMatchScore bestMatchScore = new(0, 0);
        IOpenApiPathItem? matchingTemplatePathItem = null;

        foreach (var kvp in paths)
        {
            var specPath = new PathString(kvp.Key);

            if (!IsPathMatch(specPath, requestPathString, out var isTemplatePath))
            {
                continue;
            }

            if (isTemplatePath)
            {
                var matchScore = GetTemplateMatchScore(specPath);
                if (matchScore.BetterThan(bestMatchScore))
                {
                    bestMatchScore = matchScore;
                    matchingTemplatePathItem = kvp.Value;
                }
                continue;
            }

            path = kvp.Value;
            return true;
        }

        if (matchingTemplatePathItem is not null)
        {
            path = matchingTemplatePathItem;
            return true;
        }

        path = null!;
        return false;
    }

    private static TemplateMatchScore GetTemplateMatchScore(PathString specPath)
    {
        var literalSegmentCount = 0;
        var literalPrefixCount = 0;

        for (var i = 0; i < specPath.Segments.Length; i++)
        {
            if (IsTemplateSegment(specPath.Segments[i]))
            {
                continue;
            }

            literalSegmentCount++;

            if (literalPrefixCount == i)
            {
                literalPrefixCount++;
            }
        }

        return new TemplateMatchScore(literalSegmentCount, literalPrefixCount);
    }

    private static bool IsPathMatch(
        PathString specPath,
        PathString requestPath,
        out bool isTemplatePath
    )
    {
        isTemplatePath = false;

        if (specPath.Segments.Length != requestPath.Segments.Length)
        {
            return false;
        }

        for (var i = 0; i < specPath.Segments.Length; i++)
        {
            var segment = specPath.Segments[i];

            if (IsTemplateSegment(segment))
            {
                // Is template parameter, so skip checking
                isTemplatePath = true;
                continue;
            }

            if (
                !segment.Equals(
                    requestPath.Segments[i],
                    StringComparison.InvariantCultureIgnoreCase
                )
            )
            {
                isTemplatePath = false;
                return false;
            }
        }

        return true;
    }

    private static bool IsTemplateSegment(string segment)
    {
        return segment.StartsWith('{') && segment.EndsWith('}');
    }

    private class TemplateMatchScore(int literalSegmentCount, int literalPrefixCount)
    {
        public int LiteralSegmentCount { get; } = literalSegmentCount;
        public int LiteralPrefixCount { get; } = literalPrefixCount;

        public bool BetterThan(TemplateMatchScore other)
        {
            return LiteralSegmentCount > other.LiteralSegmentCount
                || (
                    LiteralSegmentCount == other.LiteralSegmentCount
                    && LiteralPrefixCount > other.LiteralPrefixCount
                );
        }
    }
}
