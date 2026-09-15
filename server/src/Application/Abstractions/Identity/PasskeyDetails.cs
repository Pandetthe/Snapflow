namespace Snapflow.Application.Abstractions.Identity;

public sealed record PasskeyDetails(string Id, string Name, DateTimeOffset CreatedAt, bool IsBackedUp);

public sealed record PasskeyChallenge(string OptionsJson, string State);
