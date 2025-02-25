using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Reader;
using Microsoft.OpenApi.Readers;
using Shouldly;

namespace OpenApiValidate.Tests;

public class SimpleExampleTest
{
    [Fact]
    public async Task SimpleExample()
    {
        var httpClient = new HttpClient();

        // Load the OpenApiDocument
        var documentLoadResult = await OpenApiDocument.LoadAsync(
            "https://petstore.swagger.io/v2/swagger.json"
        );

        // Create the OpenApiValidator
        var validator = new OpenApiValidator(documentLoadResult.Document);

        // Perform request
        var apiResponse = await httpClient.GetAsync(
            "https://petstore.swagger.io/v2/store/inventory",
            TestContext.Current.CancellationToken
        );

        // Validate does not throw
        var validateAction = async () =>
        {
            await validator.Validate(apiResponse);
        };

        validateAction.ShouldNotThrow();
    }
}
