using Snapflow.Application.Abstractions.Identity;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Auth.External;

internal sealed class ExternalAccountSignIn(
    IUserManager userManager,
    ISignInManager signInManager,
    IAuthEmailSender emailSender,
    IAuthenticationSettings settings)
{
    private const int MaxUserNameAttempts = 100;

    public async Task<Result> SignInAsync(ExternalIdentity identity, bool? useCookies, bool? useSessionCookies, string? rememberDeviceToken)
    {
        IUser? user = await userManager.FindByLoginAsync(identity.Provider, identity.ProviderKey);

        if (user is null)
        {
            if (string.IsNullOrWhiteSpace(identity.Email))
                return Result.Failure(AuthenticationErrors.ExternalEmailMissing);

            user = await userManager.FindByEmailAsync(identity.Email);

            Result<IUser> resolved = user is null
                ? await CreateAccountAsync(identity)
                : await LinkAccountAsync(user, identity);

            if (resolved.IsFailure)
                return resolved;
            user = resolved.Value;
        }

        if (user.IsDeleted)
            return Result.Failure(AuthenticationErrors.ExternalSignInFailed);

        return await signInManager.ExternalLoginSignInAsync(identity, useCookies, useSessionCookies, rememberDeviceToken);
    }

    private async Task<Result<IUser>> LinkAccountAsync(IUser user, ExternalIdentity identity)
    {
        if (user.IsDeleted)
            return Result.Failure<IUser>(UserErrors.AccountDeleted);
        if (!identity.EmailVerified)
            return Result.Failure<IUser>(AuthenticationErrors.ExternalEmailNotVerified);

        if (!await userManager.IsEmailConfirmedAsync(user))
            return Result.Failure<IUser>(AuthenticationErrors.ExistingAccountNotConfirmed);

        Result linked = await userManager.AddLoginAsync(user, identity);
        return linked.IsSuccess ? Result.Success(user) : Result.Failure<IUser>(linked.Error);
    }

    private async Task<Result<IUser>> CreateAccountAsync(ExternalIdentity identity)
    {
        if (!settings.ExternalSignUpEnabled)
            return Result.Failure<IUser>(AuthenticationErrors.ExternalSignUpDisabled);

        string? userName = await FindFreeUserNameAsync(identity);
        if (userName is null)
            return Result.Failure<IUser>(AuthenticationErrors.ExternalSignInFailed);

        Result<IUser> created = await userManager.CreateExternalAsync(identity, userName);
        if (created.IsFailure)
            return created;

        if (identity.EmailVerified)
            return created;

        string code = await userManager.GenerateEmailConfirmationTokenAsync(created.Value);
        await emailSender.SendConfirmationLinkAsync(created.Value, code);
        return Result.Failure<IUser>(AuthenticationErrors.ExternalConfirmationSent);
    }

    private async Task<string?> FindFreeUserNameAsync(ExternalIdentity identity)
    {
        string suggestion = UserNameSuggestion.From(identity.Name, identity.Email);
        if (await userManager.FindByNameAsync(suggestion) is null)
            return suggestion;

        for (int number = 2; number <= MaxUserNameAttempts; number++)
        {
            string candidate = UserNameSuggestion.WithNumber(suggestion, number);
            if (await userManager.FindByNameAsync(candidate) is null)
                return candidate;
        }

        return null;
    }
}
