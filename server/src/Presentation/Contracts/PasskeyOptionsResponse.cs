using Snapflow.Application.Abstractions.Identity;
using System.Text.Json;

namespace Snapflow.Presentation.Contracts;

public sealed record PasskeyOptionsResponse(JsonElement Options, string State)
{
    public static PasskeyOptionsResponse From(PasskeyChallenge challenge)
    {
        using JsonDocument document = JsonDocument.Parse(challenge.OptionsJson);
        return new PasskeyOptionsResponse(document.RootElement.Clone(), challenge.State);
    }
}
