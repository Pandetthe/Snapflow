using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Application.Auth.External;
using Snapflow.Common;
using Snapflow.Domain.Users;

namespace Snapflow.Application.Auth.ExternalSignIn;

internal sealed class ExternalSignInHandler(
    ISignInManager signInManager,
    ExternalAccountSignIn accountSignIn)
    : ICommandHandler<ExternalSignInCommand>
{
    public async Task<Result> Handle(ExternalSignInCommand command, CancellationToken cancellationToken = default)
    {
        ExternalSignInTicket? ticket = await signInManager.GetExternalSignInAsync();
        if (ticket is null)
            return Result.Failure(AuthenticationErrors.ExternalSignInFailed);

        try
        {
            return await accountSignIn.SignInAsync(
                ticket.Identity,
                useCookies: ticket.IsPersistent,
                useSessionCookies: !ticket.IsPersistent,
                rememberDeviceToken: null);
        }
        finally
        {
            await signInManager.SignOutExternalAsync();
        }
    }
}
