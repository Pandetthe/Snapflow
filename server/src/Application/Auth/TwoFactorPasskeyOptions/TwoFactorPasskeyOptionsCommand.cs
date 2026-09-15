using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Auth.TwoFactorPasskeyOptions;

public sealed record TwoFactorPasskeyOptionsCommand(string? TwoFactorToken) : ICommand<PasskeyChallenge>;
