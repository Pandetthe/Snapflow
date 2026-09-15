using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Snapflow.Application.Abstractions.Identity;
using Snapflow.Common;
using Snapflow.Domain.Users;
using Snapflow.Infrastructure.Auth.Entities;
using System.Buffers.Text;

namespace Snapflow.Infrastructure.Auth.Managers;

internal sealed class AppPasskeyManager(
    UserManager<AppUser> userManager,
    IPasskeyHandler<AppUser> passkeyHandler,
    IHttpContextAccessor httpContextAccessor,
    PasskeyStateProtector stateProtector) : IPasskeyManager
{
    private const string DefaultName = "Passkey";

    private HttpContext Context => httpContextAccessor.HttpContext
        ?? throw new InvalidOperationException("Passkey operations require an HTTP request.");

    private static AppUser EnsureIsAppUser(IUser user)
    {
        return user as AppUser ?? throw new ArgumentException("User must be of type AppUser.", nameof(user));
    }

    public async Task<Result<PasskeyChallenge>> CreateRegistrationOptionsAsync(IUser user)
    {
        AppUser appUser = EnsureIsAppUser(user);
        if ((await userManager.GetPasskeysAsync(appUser)).Count >= Domain.Users.UserOptions.MaxPasskeysPerUser)
            return Result.Failure<PasskeyChallenge>(PasskeyErrors.LimitReached);

        string userId = await userManager.GetUserIdAsync(appUser);
        var userEntity = new PasskeyUserEntity
        {
            Id = userId,
            Name = appUser.Email ?? appUser.UserName ?? userId,
            DisplayName = appUser.UserName ?? appUser.Email ?? userId
        };

        PasskeyCreationOptionsResult options = await passkeyHandler.MakeCreationOptionsAsync(userEntity, Context);

        return new PasskeyChallenge(
            options.CreationOptionsJson,
            stateProtector.Protect(PasskeyStateProtector.Registration, userId, options.AttestationState));
    }

    public async Task<Result<PasskeyDetails>> RegisterAsync(IUser user, string credentialJson, string state, string? name)
    {
        AppUser appUser = EnsureIsAppUser(user);
        string userId = await userManager.GetUserIdAsync(appUser);

        PasskeyCeremony? ceremony = await stateProtector.ConsumeAsync(state, PasskeyStateProtector.Registration);
        if (ceremony is null || !string.Equals(ceremony.UserId, userId, StringComparison.Ordinal))
            return Result.Failure<PasskeyDetails>(PasskeyErrors.Expired);

        if ((await userManager.GetPasskeysAsync(appUser)).Count >= Domain.Users.UserOptions.MaxPasskeysPerUser)
            return Result.Failure<PasskeyDetails>(PasskeyErrors.LimitReached);

        PasskeyAttestationResult attestation = await passkeyHandler.PerformAttestationAsync(new PasskeyAttestationContext
        {
            HttpContext = Context,
            CredentialJson = credentialJson,
            AttestationState = ceremony.State
        });

        if (!attestation.Succeeded || !string.Equals(attestation.UserEntity.Id, userId, StringComparison.Ordinal))
            return Result.Failure<PasskeyDetails>(PasskeyErrors.RegistrationFailed);

        if (await userManager.FindByPasskeyIdAsync(attestation.Passkey.CredentialId) is not null)
            return Result.Failure<PasskeyDetails>(PasskeyErrors.AlreadyRegistered);

        attestation.Passkey.Name = string.IsNullOrWhiteSpace(name) ? DefaultName : name.Trim();

        IdentityResult added = await userManager.AddOrUpdatePasskeyAsync(appUser, attestation.Passkey);
        return added.Succeeded
            ? ToDetails(attestation.Passkey)
            : Result.Failure<PasskeyDetails>(PasskeyErrors.RegistrationFailed);
    }

    public async Task<IReadOnlyList<PasskeyDetails>> GetPasskeysAsync(IUser user)
    {
        IList<UserPasskeyInfo> passkeys = await userManager.GetPasskeysAsync(EnsureIsAppUser(user));
        return [.. passkeys.OrderBy(p => p.CreatedAt).Select(ToDetails)];
    }

    public async Task<Result> RenameAsync(IUser user, string passkeyId, string name)
    {
        AppUser appUser = EnsureIsAppUser(user);
        UserPasskeyInfo? passkey = await FindPasskeyAsync(appUser, passkeyId);
        if (passkey is null)
            return Result.Failure(PasskeyErrors.NotFound);

        passkey.Name = name.Trim();

        IdentityResult updated = await userManager.AddOrUpdatePasskeyAsync(appUser, passkey);
        return updated.Succeeded ? Result.Success() : IdentityFailure(updated);
    }

    public async Task<Result> RemoveAsync(IUser user, string passkeyId)
    {
        AppUser appUser = EnsureIsAppUser(user);
        UserPasskeyInfo? passkey = await FindPasskeyAsync(appUser, passkeyId);
        if (passkey is null)
            return Result.Failure(PasskeyErrors.NotFound);

        IdentityResult removed = await userManager.RemovePasskeyAsync(appUser, passkey.CredentialId);
        return removed.Succeeded ? Result.Success() : IdentityFailure(removed);
    }

    private async Task<UserPasskeyInfo?> FindPasskeyAsync(AppUser user, string passkeyId)
    {
        byte[] credentialId;
        try
        {
            credentialId = Base64Url.DecodeFromChars(passkeyId);
        }
        catch (FormatException)
        {
            return null;
        }

        return await userManager.GetPasskeyAsync(user, credentialId);
    }

    private static PasskeyDetails ToDetails(UserPasskeyInfo passkey) =>
        new(Base64Url.EncodeToString(passkey.CredentialId), passkey.Name ?? DefaultName, passkey.CreatedAt, passkey.IsBackedUp);

    private static Result IdentityFailure(IdentityResult result) =>
        Result.ValidationFailure(new ValidationError(
            result.Errors.Select(e => new PropertyValidationError(null, e.Code, e.Description)).ToArray()));
}
