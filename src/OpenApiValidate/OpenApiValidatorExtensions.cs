namespace OpenApiValidate;

public static class OpenApiValidatorExtensions
{
    public static async Task Validate(
        this OpenApiValidator validator,
        HttpResponseMessage httpResponse
    )
    {
        var httpRequest = httpResponse.RequestMessage!;

        var requestBody =
            httpRequest.Content != null ? await httpRequest.Content.ReadAsStringAsync() : null;

        var request = new Request(
            httpRequest.Method.ToString(),
            httpRequest.RequestUri!,
            httpRequest.Content?.Headers.ContentType?.MediaType,
            requestBody
        );

        var responseBody = await httpResponse.Content.ReadAsStringAsync();
        var response = new Response(
            (int)httpResponse.StatusCode,
            httpResponse.Content.Headers.ContentType?.MediaType,
            responseBody
        );

        validator.Validate(request, response);
    }
}
