using Microsoft.OpenApi.Models;

namespace OpenApiValidate;

internal class OpenApiSettingsValidator(
    OpenApiDocument openApiDocument,
    OpenApiValidatorSettings settings
)
{
    public void Validate()
    {
        ValidateServerAliases();
    }

    private void ValidateServerAliases()
    {
        if (settings.ServerAliases.Count == 0)
        {
            return;
        }

        if (openApiDocument.Servers == null)
        {
            throw new InvalidOperationException(
                $"Server '{settings.ServerAliases.First().Key}' not found"
            );
        }

        foreach (var serverAlias in settings.ServerAliases.Keys)
        {
            if (openApiDocument.Servers.All(s => s.Url != serverAlias))
            {
                throw new InvalidOperationException($"Server '{serverAlias}' not found");
            }
        }
    }
}
