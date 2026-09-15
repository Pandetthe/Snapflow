using Snapflow.Application.Abstractions.Messaging;

namespace Snapflow.Application.Users.Me.TwoFactor.GetStatus;

public sealed record GetTwoFactorStatusQuery : IQuery<GetTwoFactorStatusResponse>;
