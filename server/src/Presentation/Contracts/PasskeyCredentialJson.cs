using System.Text.Json;

namespace Snapflow.Presentation.Contracts;

public static class PasskeyCredentialJson
{
    public static string? From(JsonElement? credential) =>
        credential is { ValueKind: JsonValueKind.Object } value ? value.GetRawText() : null;
}
