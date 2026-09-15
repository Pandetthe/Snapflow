using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Auth.PasskeySignInOptions;

public sealed record PasskeySignInOptionsCommand : ICommand<PasskeyChallenge>;
