using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace OpenApiValidate.Tests;

public static class DocumentLoader
{
    public static async Task<OpenApiDocument> GetDocument(string filename)
    {
        var settings = new OpenApiReaderSettings();
        settings.AddYamlReader();

        var result = await OpenApiDocument.LoadAsync(File.OpenRead(filename), settings: settings);

        if (result.Diagnostic != null && result.Diagnostic.Errors.Any())
        {
            throw new Exception(
                "Invalid OpenAPI document: "
                    + string.Join(Environment.NewLine, result.Diagnostic.Errors)
            );
        }

        if (result.Document == null)
        {
            throw new Exception("Invalid OpenAPI document");
        }

        return result.Document;
    }
}
