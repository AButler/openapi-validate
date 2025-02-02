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

        var validateAction = () =>
        {
            validator.Validate(request, response);
        };

        validateAction.ShouldNotThrow();
    }

    [Fact]
    public async Task ServerAlias()
    {
        var openApiDocument = await GetDocument("TestData/Simple.yaml");

        var settings = new OpenApiValidatorSettings();
        settings.ServerAliases.Add("http://api.example.com/v1", "http://localhost/v1");

        var validator = new OpenApiResponseValidator(openApiDocument, settings);

        var request = new Request("GET", new Uri("http://localhost/v1/users"));
        var response = new Response(200, "application/json", """["user1"]""");

        var validateAction = () =>
        {
            validator.Validate(request, response);
        };

        validateAction.ShouldNotThrow();
    }

    [Fact]
    public async Task NoServers()
    {
        var openApiDocument = await GetDocument("TestData/NoServers.yaml");

        var validator = new OpenApiResponseValidator(openApiDocument);

        var request = new Request("GET", new Uri("http://localhost/users"));
        var response = new Response(200, "application/json", """["user1"]""");

        var validateAction = () =>
        {
            validator.Validate(request, response);
        };

        validateAction.ShouldNotThrow();
    }

    [Fact]
    public async Task Petstore_PutPet()
    {
        var openApiDocument = await GetDocument("TestData/Petstore.yaml");

        var validator = new OpenApiResponseValidator(openApiDocument);

        var request = new Request(
            "PUT",
            new Uri("https://petstore3.swagger.io/api/v3/pet"),
            "application/json",
            """{"name": "dog", "photoUrls": []}"""
        );

        var response = new Response(
            200,
            "application/json",
            """{"id": 5, "name": "dog", "photoUrls": []}"""
        );

        var validateAction = () =>
        {
            validator.Validate(request, response);
        };

        validateAction.ShouldNotThrow();
    }

    [Fact]
    public async Task Petstore_PutPet_Error()
    {
        var openApiDocument = await GetDocument("TestData/Petstore.yaml");

        var validator = new OpenApiResponseValidator(openApiDocument);

        var request = new Request(
            "PUT",
            new Uri("https://petstore3.swagger.io/api/v3/pet"),
            "application/json",
            """{"name": "dog", "photoUrls": []}"""
        );

        var response = new Response(500);

        var validateAction = () =>
        {
            validator.Validate(request, response);
        };

        validateAction.ShouldNotThrow();
    }

    [Fact]
    public async Task Petstore_DeletePet()
    {
        var openApiDocument = await GetDocument("TestData/Petstore.yaml");

        var validator = new OpenApiResponseValidator(openApiDocument);

        var request = new Request(
            "DELETE",
            new Uri("https://petstore3.swagger.io/api/v3/pet/Pet1")
        );
        var response = new Response(201);

        var validateAction = () =>
        {
            validator.Validate(request, response);
        };

        validateAction.ShouldNotThrow();
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
