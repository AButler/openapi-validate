using Json.Schema;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace OpenApiValidate;

internal static class OpenApiExtensions
{
    public static JsonSchema ToJsonSchema(this OpenApiSchema schema)
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
}
