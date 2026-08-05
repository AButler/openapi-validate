using Shouldly;

namespace OpenApiValidate.Tests;

public class DiscriminatorPropertiesTests
{
    [Fact]
    public async Task DiscriminatorProperties()
    {
        using var httpClient = new HttpClient();

        var openApiDocument = await DocumentLoader.GetDocument("TestData/Drinks.yaml");

        var validator = new OpenApiValidator(openApiDocument);

        var request = new Request("GET", new Uri("https://api.drinks.example.dev/api/menu/items"));
        var response = new Response(200, "application/json", GetResponseBody("menu-items.json"));

        var validateAction = () =>
        {
            validator.Validate(request, response);
        };

        validateAction.ShouldNotThrow();
    }

    private string GetResponseBody(string responseFilename)
    {
        return File.ReadAllText($"Responses/{responseFilename}");
    }
}
