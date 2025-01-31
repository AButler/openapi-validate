using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Reader;
using Microsoft.OpenApi.Readers;
using Shouldly;

namespace OpenApiValidate.Tests;

public class ResponseValidatorTests
{
    [Fact]
    public async Task Simple()
    {
        var openApiDocument = await GetDocument("TestData/Simple.yaml");

        var validator = new OpenApiResponseValidator(openApiDocument);

        var request = new Request("GET", new Uri("http://api.example.com/v1/users"));
        var response = new Response(200, "application/json", """["user1"]""");

        var valid = validator.TryValidate(request, response);

        valid.ShouldBeTrue();
    }

    [Fact]
    public async Task Petstore_PutPet()
    {
        var openApiDocument = await GetDocument("TestData/Petstore.yaml");

        var validator = new OpenApiResponseValidator(openApiDocument);

        var request = new Request("PUT", new Uri("https://petstore3.swagger.io/api/v3/pet"));
        var response = new Response(
            200,
            "application/json",
            """{"id": 5, "name": "dog", "photoUrls": []}"""
        );

        var valid = validator.TryValidate(request, response);

        valid.ShouldBeTrue();
    }

    private static async Task<OpenApiDocument> GetDocument(string filename)
    {
        OpenApiReaderRegistry.RegisterReader(OpenApiConstants.Yaml, new OpenApiYamlReader());
        var result = await OpenApiDocument.LoadAsync(File.OpenRead(filename));

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
