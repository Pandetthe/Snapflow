using Snapflow.Common;

namespace Snapflow.Domain.Users;

public sealed record TwoFactorRequiredError(string? TwoFactorToken, bool PasskeyAvailable)
    : Error(UserErrors.SignInTwoFactorRequired.Code, UserErrors.SignInTwoFactorRequired.Description, ErrorType.Unauthorized);
