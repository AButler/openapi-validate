using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Reader;
using Microsoft.OpenApi.Readers;
using Shouldly;

namespace OpenApiValidate.Tests;

public class ResponseValidatorTests
{
    [Fact]
    public async Task Test()
    {
        var openApiDocument = await GetDocument("TestData/Simple.yaml");

        var validator = new OpenApiResponseValidator(openApiDocument);

        var request = new Request("GET", new Uri("http://api.example.com/v1/users"));
        var response = new Response(200, "application/json", """["user1"]""");

        var valid = validator.TryValidate(request, response);

        valid.ShouldBeTrue();
    }

    private static async Task<OpenApiDocument> GetDocument(string filename)
    {
        OpenApiReaderRegistry.RegisterReader(OpenApiConstants.Yaml, new OpenApiYamlReader());
        var result = await OpenApiDocument.LoadAsync(File.OpenRead("TestData/Simple.yaml"));

        if (result.Diagnostic.Errors.Any())
        {
            throw new Exception(
                "Invalid OpenAPI document: "
                    + string.Join(Environment.NewLine, result.Diagnostic.Errors)
            );
        }

        return result.Document;
    }
}
