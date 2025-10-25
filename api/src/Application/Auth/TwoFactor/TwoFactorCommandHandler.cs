//using Snapflow.Application.Abstractions.Identity;
//using Snapflow.Application.Abstractions.Messaging;
//using Snapflow.Common;

//namespace Snapflow.Application.Users.Auth.TwoFactor;

//internal sealed class TwoFactorCommandHandler(
//    ISignInManager signInManager,
//    IUserManager userManager,
//    IUserContext userContext) : ICommandHandler<TwoFactorCommand>
//{
//    public async Task<Result> Handle(TwoFactorCommand command, CancellationToken cancellationToken = default)
//    {
//        var user = await userContext.GetUserAsync();
//        if (command.Enable == true)
//        {
//            if (command.ResetSharedKey)
//            {
//                return CreateValidationProblem("CannotResetSharedKeyAndEnable",
//                    "Resetting the 2fa shared key must disable 2fa until a 2fa token based on the new shared key is validated.");
//            }

//            if (string.IsNullOrEmpty(command.TwoFactorCode))
//            {
//                return CreateValidationProblem("RequiresTwoFactor",
//                    "No 2fa token was provided by the request. A valid 2fa token is required to enable 2fa.");
//            }

//            if (!await userManager.VerifyTwoFactorTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, command.TwoFactorCode))
//            {
//                return CreateValidationProblem("InvalidTwoFactorCode",
//                    "The 2fa token provided by the request was invalid. A valid 2fa token is required to enable 2fa.");
//            }

//            await userManager.SetTwoFactorEnabledAsync(user, true);
//        }
//        else if (command.Enable == false || command.ResetSharedKey)
//        {
//            await userManager.SetTwoFactorEnabledAsync(user, false);
//        }

//        if (command.ResetSharedKey)
//        {
//            await userManager.ResetAuthenticatorKeyAsync(user);
//        }

//        string[]? recoveryCodes = null;
//        if (command.ResetRecoveryCodes || (command.Enable == true && await userManager.CountRecoveryCodesAsync(user) == 0))
//        {
//            var recoveryCodesEnumerable = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
//            recoveryCodes = recoveryCodesEnumerable?.ToArray();
//        }

//        if (command.ForgetMachine)
//        {
//            await signInManager.ForgetTwoFactorClientAsync();
//        }

//        var key = await userManager.GetAuthenticatorKeyAsync(user);
//        if (string.IsNullOrEmpty(key))
//        {
//            await userManager.ResetAuthenticatorKeyAsync(user);
//            key = await userManager.GetAuthenticatorKeyAsync(user);

//            if (string.IsNullOrEmpty(key))
//            {
//                throw new NotSupportedException("The user manager must produce an authenticator key after reset.");
//            }
//        }

//        return TypedResults.Ok(new TwoFactorResponse
//        {
//            SharedKey = key,
//            RecoveryCodes = recoveryCodes,
//            RecoveryCodesLeft = recoveryCodes?.Length ?? await userManager.CountRecoveryCodesAsync(user),
//            IsTwoFactorEnabled = await userManager.GetTwoFactorEnabledAsync(user),
//            IsMachineRemembered = await signInManager.IsTwoFactorClientRememberedAsync(user),
//        });
//    }
//}