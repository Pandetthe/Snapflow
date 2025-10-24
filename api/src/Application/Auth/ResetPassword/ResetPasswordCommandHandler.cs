using Snapflow.Application.Abstractions.Identity;
using Snapflow.Application.Abstractions.Messaging;
using Snapflow.Common;
using System.Text;

namespace Snapflow.Application.Auth.ResetPassword;

internal sealed class ResetPasswordCommandHandler(
    IUserManager userManager) : ICommandHandler<ResetPasswordCommand>
{
    public async Task<Result> Handle(ResetPasswordCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        // TODO: Implement
        return Result.Success();
    }
}